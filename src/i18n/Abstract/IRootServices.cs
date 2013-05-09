using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n
{
    /// <summary>
    /// Describes the set of i18n localizing services registered for the current AppDomain.
    /// Typically these are used by objects at the root of the dependency-injection graph,
    /// such as an HttpModule.
    /// </summary>
    public interface IRootServices
    {
        IUrlLocalizer UrlLocalizerForApp { get; }
        ITextLocalizer TextLocalizerForApp { get; }
        IEarlyUrlLocalizer EarlyUrlLocalizerForApp { get; }
        INuggetLocalizer NuggetLocalizerForApp { get; }
    }
}
