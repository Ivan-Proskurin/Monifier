using System;

namespace Monifier.BusinessLogic.Contract.Common
{
    public interface ITimeService
    {
        DateTime ClientLocalNow { get; }
    }
}