namespace AppmetrCS.Actions
{
	#region using directives

	using System;
	using System.Runtime.Serialization;

	#endregion

	[DataContract]
	public class AttachEntityAttributes : AppMetrAction
	{
		private const String ACTION = "attachEntityAttributes";
		private String entityName;
		private String entityValue;

		public AttachEntityAttributes(String entityName, String entityValue) : base(ACTION)
		{
			this.entityName = entityName;
			this.entityValue = entityValue;
		}
		
		public override Int32 CalcApproximateSize()
		{
			return base.CalcApproximateSize()
			       + GetStringLength(entityName)
			       + GetStringLength(entityValue);
		}
                
		public override String ToString()
		{
			return $"{base.ToString()}, " +
			       $"EntityName: {entityName}, " +
			       $"EntityValue: {entityValue}";
		}
	}
}