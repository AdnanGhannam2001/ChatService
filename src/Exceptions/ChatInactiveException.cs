using ChatService.Data.Models;
using PR2.Shared.Common;

namespace ChatService.Exceptions;

internal class ChatInactiveException : ExceptionBase
{
    public ChatInactiveException()
        : base(nameof(Chat.IsActive), "Chat is not active any more") { }
}