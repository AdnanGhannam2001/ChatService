using System.ComponentModel.DataAnnotations;

namespace ChatService.Dtos;

public record SendMessageRequest([Required, MaxLength(1000)] string Content);