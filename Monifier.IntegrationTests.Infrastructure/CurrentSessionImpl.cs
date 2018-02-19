﻿using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Model.Auth;
using Monifier.DataAccess.Model.Auth;

namespace Monifier.IntegrationTests.Infrastructure
{
    public class CurrentSessionImpl : ICurrentSession
    {
        public CurrentSessionImpl() : this(null)
        {
        }

        public CurrentSessionImpl(User user)
        {
            CurrentUser = user;
        }
        
        public User CurrentUser { get; }

        public bool IsAuthenticated => CurrentUser != null;
        
        public int UserId => CurrentUser?.Id ?? throw new AuthException("Not authenticated");
    }
}