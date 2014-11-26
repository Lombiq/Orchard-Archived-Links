using Lombiq.ArchivedLinks.Models;
using Orchard;
using Orchard.FileSystems.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lombiq.ArchivedLinks.Services
{
    public interface ILinkManager : IDependency
    {
        //void SaveLink(string originalUrl);
        string SaveLink(LinkPart linkPart);

        string GetPublicUrl(int id);

        string CheckJumpUrl(string url);
    }
}
