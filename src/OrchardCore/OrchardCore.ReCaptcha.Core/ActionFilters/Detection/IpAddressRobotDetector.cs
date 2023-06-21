using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.ReCaptcha.Configuration;

namespace OrchardCore.ReCaptcha.ActionFilters.Detection
{
    public class IpAddressRobotDetector : IDetectRobots
    {
        private const string IpAddressAbuseDetectorCacheKey = "IpAddressRobotDetector";

        private readonly IMemoryCache _memoryCache;
        private readonly IOrchardHelper _orchardHelper;
        private readonly ReCaptchaSettings _settings;

        public IpAddressRobotDetector(
            IOrchardHelper orchardHelper,
            IMemoryCache memoryCache,
            IOptions<ReCaptchaSettings> settingsAccessor)
        {
            _orchardHelper = orchardHelper;
            _memoryCache = memoryCache;
            _settings = settingsAccessor.Value;
        }

        public void IsNotARobot()
        {
            var ipAddressKey = GetIpAddressCacheKey();
            _memoryCache.Remove(ipAddressKey);
        }

        private string GetIpAddressCacheKey()
        {
            return $"{IpAddressAbuseDetectorCacheKey}:{_orchardHelper.GetClientIpAddress()}";
        }

        public RobotDetectionResult DetectRobot()
        {
            var ipAddressKey = GetIpAddressCacheKey();
            var faultyRequestCount = _memoryCache.GetOrCreate(ipAddressKey, fact => 0);

            return new RobotDetectionResult()
            {
                IsRobot = faultyRequestCount > _settings.DetectionThreshold
            };
        }

        public void FlagAsRobot()
        {
            var ipAddressKey = GetIpAddressCacheKey();

            // this has race conditions, but it's ok
            var faultyRequestCount = _memoryCache.GetOrCreate(ipAddressKey, fact => 0);
            faultyRequestCount++;
            _memoryCache.Set(ipAddressKey, faultyRequestCount);
        }
    }
}
