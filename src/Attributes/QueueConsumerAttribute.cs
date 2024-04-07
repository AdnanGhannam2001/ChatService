namespace ChatService.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal class QueueConsumerAttribute : Attribute {
    public string? Name { get; set; }
}