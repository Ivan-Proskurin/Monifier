using System;

namespace Monifier.BusinessLogic.Contract.Common
{
    public interface ITimeProvider
    {
        DateTime ClientLocalNow { get; }
    }
}