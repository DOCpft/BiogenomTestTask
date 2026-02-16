using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Interfaces
{
    public interface IImageDownloader
    {
        Task<byte[]> DownloadAsync(string url);
    }
}
