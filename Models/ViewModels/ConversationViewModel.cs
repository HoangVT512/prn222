using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using InternManagement.Models;

namespace InternManagement.ViewModels
{
    public class ConversationListViewModel
    {
        public List<ConversationViewModel> Conversations { get; set; }
        public int CurrentUserId { get; set; }
    }

    public class ConversationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsGroup { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public string ProfileImage { get; set; }
        public List<UserBasicInfoViewModel> Members { get; set; }
    }

    public class UserBasicInfoViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ProfileImage { get; set; }
    }

    public class ChatViewModel
    {
        public int ConversationId { get; set; }
        public string ConversationName { get; set; }
        public bool IsGroup { get; set; }
        public List<MessageViewModel> Messages { get; set; }
        public List<UserBasicInfoViewModel> Members { get; set; }
        public int CurrentUserId { get; set; }
        public string NewMessage { get; set; }
    }

    public class MessageViewModel
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderProfileImage { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCurrentUserMessage { get; set; }
    }
    public class NewConversationViewModel
    {
        [Display(Name = "Group Chat Name (Leave empty for private chat)")]
        public string Name { get; set; }

        public bool IsGroup => !string.IsNullOrEmpty(Name);

        [Required(ErrorMessage = "Select at least one participant")]
        public List<int> SelectedUserIds { get; set; }

        public List<UserSelectionViewModel> AvailableUsers { get; set; }
    }

    public class UserSelectionViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfileImage { get; set; }
        public string Role { get; set; }
        public bool IsSelected { get; set; }
    }
}