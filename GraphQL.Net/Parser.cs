using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.FSharp.Collections;

namespace GraphQL.Net
{
    public static class Parser
    {
        public static Query Parse( string query )
        {
            var parsedQuery = GraphQLParser.parse(query);
            if( parsedQuery.Value == null )
                throw new Exception( "i dunno man" );
            if( !parsedQuery.Value.IsQueryOperation ) throw new InvalidOperationException( "Only query operation are supported..." );

            var op = (GraphQLParser.Definition.QueryOperation)parsedQuery.Value;
            return ExpandFragments( new Query
            {
                Name = op.Item.Item1,
                Inputs = GetInputs( op.Item.Item2 ),
                Fields = WalkSelection( op.Item.Item3 ),
                Fragments = GetFragments( op.Item.Item4 )
            } );
        }

        private static List<Fragment> GetFragments( FSharpList<GraphQLParser.Fragment> item4 )
        {
            List<Fragment> list = new List<Fragment>();
            foreach( var fragmentItem in item4 )
            {
                var f = new Fragment
                {
                    Name = fragmentItem.Item1,
                    AppliedEntity = fragmentItem.Item2,
                    Fields = WalkSelection( fragmentItem.Item3 )
                };
                list.Add( f );
            }
            return list;
        }

        private static List<Input> GetInputs( IEnumerable<Tuple<string, GraphQLParser.Input>> inputs )
        {
            return inputs.Select( i => new Input
            {
                Name = i.Item1,
                Value = GetInputValue( i.Item2 )
            } ).ToList();
        }

        private static object GetInputValue( GraphQLParser.Input input )
        {
            if( input.IsBoolean ) return ((GraphQLParser.Input.Boolean)input).Item;
            if( input.IsFloat ) return ((GraphQLParser.Input.Float)input).Item;
            if( input.IsInt ) return ((GraphQLParser.Input.Int)input).Item;
            if( input.IsString ) return ((GraphQLParser.Input.String)input).Item;
            throw new Exception( "Shouldn't be here" );
        }

        private static List<Field> WalkSelection( IEnumerable<GraphQLParser.Selection> selection )
        {
            return selection.Select( f => new Field
            {
                Name = f.name,
                Alias = f.alias?.Value ?? f.name,
                Fields = WalkSelection( f.selectionSet ),
                FragmentRef = f.fragmentIdentifier != null ? f.name : null
            } ).ToList();
        }

        private static Query ExpandFragments( Query query )
        {
            var fragmentMap = query.Fragments.ToDictionary( x => x.Name, y => y );

            IncludeFragment( query.Fields, fragmentMap );
            return query;
        }

        static void IncludeFragment( List<Field> fields, IDictionary<string, Fragment> fragmentMap )
        {
            List<Field> pendingF = new List<Field>();
            foreach( Field field in fields )
            {
                if( !String.IsNullOrEmpty( field.FragmentRef ) )
                {
                    Fragment existingFragment;
                    if( !fragmentMap.TryGetValue( field.FragmentRef, out existingFragment ) )
                        throw new ArgumentException( String.Format( "The fragment {0} is not found in the query...", fragmentMap ) );

                    pendingF.AddRange( existingFragment.Fields );
                }
                IncludeFragment( field.Fields, fragmentMap );
            }
            fields.RemoveAll( x => x.FragmentRef != null );
            fields.AddRange( pendingF );
        }
    }
}
