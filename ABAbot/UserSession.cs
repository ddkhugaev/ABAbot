public class UserSession
{
	public string? FullName { get; set; }
	public string? Love { get; set; }
	public string? GoodAt { get; set; }
	public string? PaidFor { get; set; }
	public string? WorldNeeds { get; set; }
	public Step Step { get; set; } = Step.AskName;
}

public enum Step
{
	AskName,
	MainMenu,
	Question1,
	Question2,
	Question3,
	Question4
}
