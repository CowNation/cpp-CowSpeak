namespace CowSpeak
{
	internal class Conversion
	{
		public static bool IsCompatible(Type from, Type to)
		{
			return to == Type.Any || from == to || (from == Type.Integer && to == Type.Decimal) || (from == Type.Decimal && to == Type.Integer) || (from == Type.Boolean && to == Type.Integer) || (from == Type.Character && to == Type.Integer) || (from == Type.Integer && to == Type.Character) || (from == Type.Integer && to == Type.Integer64) || (from == Type.Integer64 && to == Type.Integer) || (from == Type.DecimalArray && to == Type.IntegerArray) || (from == Type.IntegerArray && to == Type.DecimalArray);
		}
	}
}