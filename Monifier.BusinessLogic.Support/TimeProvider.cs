using System;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Common;

namespace Monifier.BusinessLogic.Support
{
    public class TimeProvider : ITimeProvider
    {
        private readonly ICurrentSession _currentSession;

        public TimeProvider(ICurrentSession currentSession)
        {
            _currentSession = currentSession;
        }

        public DateTime ClientLocalNow => DateTime.UtcNow.AddMinutes(_currentSession.TimeZoneOffest);
    }
}