﻿using CMS.Membership;
using Kentico.Membership;

namespace Core.Services
{
    public interface IBaselineUserMapper<TUser, TGenericUser> where TUser : ApplicationUser, new() where TGenericUser : User, new()
    {
        Task<TGenericUser> ToUser(MemberInfo memberInfo);

        Task<TUser> ToApplicationUser(TGenericUser user);
    }
}
