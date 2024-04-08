using Microsoft.AspNetCore.Authorization;
using PR2.Shared.Enums;

namespace ChatService.Policies.Requirements;

internal record MembershipRequirement(MemberRoleTypes MinimalRole) : IAuthorizationRequirement;