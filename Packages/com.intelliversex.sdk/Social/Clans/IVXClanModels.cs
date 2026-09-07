using System;
using System.Collections.Generic;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Represents a clan in the IntelliVerseX SDK.
    /// </summary>
    [Serializable]
    public sealed class IVXClanData
    {
        public string ClanId;
        public string Name;
        public string Description;
        public string AvatarUrl;
        public int MemberCount;
        public int MaxMembers;
        public int Level;
        public int Experience;
        public bool IsOpen;
        public string CreatorId;
        public string UserRole;
        public string JoinedAt;
    }

    /// <summary>
    /// Represents a clan member.
    /// </summary>
    [Serializable]
    public sealed class IVXClanMemberData
    {
        public string UserId;
        public string Username;
        public string DisplayName;
        public int RoleState;
        public bool IsOnline;
        public int WeeklyContribution;
    }

    /// <summary>
    /// Represents the result of a clan browse operation.
    /// </summary>
    [Serializable]
    public sealed class IVXClanBrowseResult
    {
        public List<IVXClanData> Clans = new List<IVXClanData>();
        public string ErrorMessage;

        public bool IsSuccess => string.IsNullOrWhiteSpace(ErrorMessage);
    }

    /// <summary>
    /// Represents the result of a clan operation.
    /// </summary>
    [Serializable]
    public sealed class IVXClanOperationResult
    {
        public bool IsSuccess;
        public string ErrorMessage;
        public IVXClanData Clan;

        public static IVXClanOperationResult Success(IVXClanData clan = null)
        {
            return new IVXClanOperationResult
            {
                IsSuccess = true,
                Clan = clan
            };
        }

        public static IVXClanOperationResult Failure(string errorMessage)
        {
            return new IVXClanOperationResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }

    [Serializable]
    internal sealed class IVXGetUserGroupsPayload
    {
        public string gameId;
    }

    [Serializable]
    internal sealed class IVXCreateClanPayload
    {
        public string gameId;
        public string name;
        public string description;
        public int maxCount;
        public bool open;
        public string groupType;
    }

    [Serializable]
    internal sealed class IVXCreateClanResponse
    {
        public bool success;
        public string groupId;
        public string name;
        public string description;
        public bool open;
        public int maxCount;
        public string createdAt;
        public string error;
    }

    [Serializable]
    internal sealed class IVXUserGroupsResponse
    {
        public bool success;
        public string userId;
        public string gameId;
        public IVXGroupInfo[] groups;
        public string error;
    }

    [Serializable]
    internal sealed class IVXGroupInfo
    {
        public string groupId;
        public string name;
        public string description;
        public int memberCount;
        public int maxCount;
        public bool open;
        public int level;
        public int xp;
        public string role;
        public string joinedAt;
    }
}
