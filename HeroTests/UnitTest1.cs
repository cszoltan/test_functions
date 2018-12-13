using HeroApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace HeroTests
{
    public class UnitTest1
    {
        private readonly ILogger logger = NullLoggerFactory.Instance.CreateLogger("Test");

        [Fact]
        public void GetHeroes()
        {

            var request = GenerateHttpRequest();
            var response = Heroes.GetData(request, "", logger);

            
        }

        private DefaultHttpRequest GenerateHttpRequest()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            return request;
        }
    }
}
