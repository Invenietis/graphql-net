﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GraphQL.Net
{
    internal static class Executor<TContext>
    {
        public static IDictionary<string, object> Execute<TArgs, TEntity>( GraphQLSchema<TContext> schema, GraphQLQuery<TContext, TArgs, TEntity> gqlQuery, Query query )
        {
            var context = schema.ContextCreator();
            var results = Execute(context, gqlQuery, query);
            (context as IDisposable)?.Dispose();
            return results;
        }

        public static IDictionary<string, object> Execute<TArgs, TEntity>( TContext context, GraphQLQuery<TContext, TArgs, TEntity> gqlQuery, Query query )
        {
            var args = TypeHelpers.GetArgs<TArgs>(query.Inputs);
            var queryableFuncExpr = gqlQuery.QueryableExprGetter(args);
            var replaced = (Expression<Func<TContext, IQueryable<TEntity>>>)ParameterReplacer.Replace(queryableFuncExpr, queryableFuncExpr.Parameters[0], GraphQLSchema<TContext>.DbParam);
            var fieldMaps = query.Fields.Select(f => MapField(f, gqlQuery.Type)).ToList();
            var selectorW = GetSelector<TEntity>(fieldMaps);
            var gqlType = selectorW.GqlSelectorType;
            var selector = selectorW.Lambda;

            var selectorExpr = Expression.Quote(selector);
            var call = Expression.Call(typeof(Queryable), "Select", new[] { typeof(TEntity), gqlType }, replaced.Body, selectorExpr);
            //var expr = (Expression<Func<TContext, IQueryable>>)Expression.Lambda(call, GraphQLSchema<TContext>.DbParam);
            // var transformed = expr.Compile()(context);
            var expr = Expression.Lambda(call, GraphQLSchema<TContext>.DbParam);
            var transformed = (IQueryable<IGQLQueryObject>) expr.Compile().DynamicInvoke( context );

            object results;
            switch( gqlQuery.ResolutionType )
            {
                case ResolutionType.Unmodified:
                    throw new Exception( "Queries cannot have unmodified resolution. May change in the future." );
                case ResolutionType.ToList:
                    results = transformed.ToList().Select( o => MapResults( o, fieldMaps ) );
                    break;
                case ResolutionType.FirstOrDefault:
                    results = MapResults( transformed.FirstOrDefault(), fieldMaps );
                    break;
                case ResolutionType.First:
                    results = MapResults( transformed.First(), fieldMaps );
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Dictionary<string, object> { { "data", results } };
        }

        private static IDictionary<string, object> MapResults( IGQLQueryObject queryObject, IEnumerable<FieldMap> fieldMaps )
        {
            if( queryObject == null ) // TODO: Check type non-null and throw exception
                return null;
            var n = 1;
            var dict = new Dictionary<string, object>();
            var type = queryObject.GetType();
            foreach( var map in fieldMaps )
            {
                var key = map.ParsedField.Alias;
                var mappedFieldName = $"Field{n}";
                var obj = type.GetProperty(mappedFieldName).GetGetMethod().Invoke(queryObject, new object[] { });
                var queryObj = obj as GQLQueryObject0;
                if( map.Children.Any() && queryObj != null )
                {
                    dict.Add( key, MapResults( queryObj, map.Children ) );
                }
                else
                {
                    dict.Add( key, obj );
                }
                n++;
            }
            return dict;
        }

        struct Selector
        {
            public Selector( Type selectorType, LambdaExpression lambda )
            {
                GqlSelectorType = selectorType;
                Lambda = lambda;
            }
            public LambdaExpression Lambda;
            public Type GqlSelectorType;
        }

        private static Selector GetSelector<TEntity>( List<FieldMap> fieldMaps )
        {
            var parameter = Expression.Parameter(typeof(TEntity), "p");
            var init = GetMemberInit(fieldMaps, parameter);

            return new Selector( GQLQueryObjectSelector.SelectGQLQueryObject( fieldMaps.Count ), Expression.Lambda( init, parameter ) );
        }

        private static FieldMap MapField( Field field, GraphQLType type )
        {
            var schemaField = type.Fields.First(f => f.Name == field.Name);
            return new FieldMap
            {
                ParsedField = field,
                SchemaField = schemaField,
                Children = field.Fields.Select( f => MapField( f, schemaField.Type ) ).ToList(),
            };
        }

        private static MemberInitExpression GetMemberInit( IList<FieldMap> maps, Expression baseBindingExpr )
        {
            var count = maps.Count;
            var toType = GQLQueryObjectSelector.SelectGQLQueryObject( count ); // TODO: Get type based on number of fields
            var fieldCount = count; // TODO: see above
            var bindings = maps.Select((map, i) => GetBinding(map, toType, baseBindingExpr, i + 1)).ToList();

            bindings.AddRange( Enumerable.Range( bindings.Count + 1, fieldCount - bindings.Count ).Select( i => GetEmptyBinding( toType, i ) ) );
            return Expression.MemberInit( Expression.New( toType ), bindings );
        }

        // Stupid EF limitation
        private static MemberBinding GetEmptyBinding( Type toType, int n )
        {
            return Expression.Bind( toType.GetMember( $"Field{n}" )[0], Expression.Constant( 0 ) );
        }

        private static MemberBinding GetBinding( FieldMap map, Type toType, Expression baseBindingExpr, int n )
        {
            var mapFieldName = $"Field{n}";
            var toMember = toType.GetMember(mapFieldName)[0];
            var expr = map.SchemaField.GetExpression(new List<Input>()); // TODO: real inputs
            var replacedBase = ParameterReplacer.Replace(expr.Body, expr.Parameters[1], baseBindingExpr);
            var replacedContext = ParameterReplacer.Replace(replacedBase, expr.Parameters[0], GraphQLSchema<TContext>.DbParam);
            if( !map.Children.Any() )
            {
#if DNX
                if( map.SchemaField.Type.CLRType.IsPrimitive )
                    replacedContext = Expression.Convert( replacedContext, typeof( object ) );
#endif
                return Expression.Bind( toMember, replacedContext );
            }
            var memberInit = GetMemberInit(map.Children, replacedContext);
            return Expression.Bind( toType.GetMember( mapFieldName )[0], memberInit );
        }
    }

    internal class FieldMap
    {
        public Field ParsedField;
        public GraphQLField SchemaField;
        public List<FieldMap> Children;
    }
}
