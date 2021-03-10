namespace SC
{
	/// 프로그램 내에 쓰이는 상수 값 모음 스태틱 클래스 : Network
	public static partial class Consts
	{
		public static readonly int PACKET_HEADER_SIZE = sizeof(ushort) + sizeof(ushort);
		public static readonly int PACKET_BUFFER_MAX_SIZE = ushort.MaxValue;
		public static readonly int READ_BUFFER_MAX_SIZE = 1024 * 1024 * 1;
		public static readonly int WRITE_BUFFER_MAX_SIZE = 1024 * 1024 * 4;
	}
}
