
using System;

namespace GraphQL.Net
{
	public interface IGQLQueryObject
	{
	}
	public class GQLQueryObject0 : IGQLQueryObject { }

	public class GQLQueryObject1 : GQLQueryObject0
	{
		public object Field1 { get; set; }
	}
	public class GQLQueryObject2 : GQLQueryObject1
	{
		public object Field2 { get; set; }
	}
	public class GQLQueryObject3 : GQLQueryObject2
	{
		public object Field3 { get; set; }
	}
	public class GQLQueryObject4 : GQLQueryObject3
	{
		public object Field4 { get; set; }
	}
	public class GQLQueryObject5 : GQLQueryObject4
	{
		public object Field5 { get; set; }
	}
	public class GQLQueryObject6 : GQLQueryObject5
	{
		public object Field6 { get; set; }
	}
	public class GQLQueryObject7 : GQLQueryObject6
	{
		public object Field7 { get; set; }
	}
	public class GQLQueryObject8 : GQLQueryObject7
	{
		public object Field8 { get; set; }
	}
	public class GQLQueryObject9 : GQLQueryObject8
	{
		public object Field9 { get; set; }
	}
	public class GQLQueryObject10 : GQLQueryObject9
	{
		public object Field10 { get; set; }
	}
	public class GQLQueryObject11 : GQLQueryObject10
	{
		public object Field11 { get; set; }
	}
	public class GQLQueryObject12 : GQLQueryObject11
	{
		public object Field12 { get; set; }
	}
	public class GQLQueryObject13 : GQLQueryObject12
	{
		public object Field13 { get; set; }
	}
	public class GQLQueryObject14 : GQLQueryObject13
	{
		public object Field14 { get; set; }
	}
	public class GQLQueryObject15 : GQLQueryObject14
	{
		public object Field15 { get; set; }
	}
	public class GQLQueryObject16 : GQLQueryObject15
	{
		public object Field16 { get; set; }
	}
	public class GQLQueryObject17 : GQLQueryObject16
	{
		public object Field17 { get; set; }
	}
	public class GQLQueryObject18 : GQLQueryObject17
	{
		public object Field18 { get; set; }
	}
	public class GQLQueryObject19 : GQLQueryObject18
	{
		public object Field19 { get; set; }
	}
	public class GQLQueryObject20 : GQLQueryObject19
	{
		public object Field20 { get; set; }
	}
	
	
    public static class GQLQueryObjectSelector
    {
		static public Type SelectGQLQueryObject( int count )
		{
			switch(count)
			{
				case 1 :return typeof( GQLQueryObject1 );
				case 2 :return typeof( GQLQueryObject2 );
				case 3 :return typeof( GQLQueryObject3 );
				case 4 :return typeof( GQLQueryObject4 );
				case 5 :return typeof( GQLQueryObject5 );
				case 6 :return typeof( GQLQueryObject6 );
				case 7 :return typeof( GQLQueryObject7 );
				case 8 :return typeof( GQLQueryObject8 );
				case 9 :return typeof( GQLQueryObject9 );
				case 10 :return typeof( GQLQueryObject10 );
				case 11 :return typeof( GQLQueryObject11 );
				case 12 :return typeof( GQLQueryObject12 );
				case 13 :return typeof( GQLQueryObject13 );
				case 14 :return typeof( GQLQueryObject14 );
				case 15 :return typeof( GQLQueryObject15 );
				case 16 :return typeof( GQLQueryObject16 );
				case 17 :return typeof( GQLQueryObject17 );
				case 18 :return typeof( GQLQueryObject18 );
				case 19 :return typeof( GQLQueryObject19 );
				case 20 :return typeof( GQLQueryObject20 );
				default: throw new NotSupportedException();
			}
		}
    }
}