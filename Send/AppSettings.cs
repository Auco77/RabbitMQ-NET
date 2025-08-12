namespace Send
{
	internal class RabbitmqSettings
	{
		public string? HostName { get; set; }
		public string? UserName { get; set; }
		public string? Password { get; set; }

		public override string ToString() => $"{HostName}:{UserName}:{Password}";
	}
}
