using System.Collections.Generic;

namespace LetsTrace.Samplers
{
    // GuaranteedThroughputProbabilisticSampler is a sampler that leverages both ProbabilisticSampler and
    // RateLimitingSampler. The RateLimitingSampler is used as a guaranteed lower bound sampler such that
    // every operation is sampled at least once in a time interval defined by the lowerBound. ie a lowerBound
    // of 1.0 / (60 * 10) will sample an operation at least once every 10 minutes.
    public class GuaranteedThroughputProbabilisticSampler : ISampler
    {
        private ISampler _probabilisticSampler;
        private ISampler _rateLimitingSampler;

        public GuaranteedThroughputProbabilisticSampler(double samplingRate, double lowerBound)
            : this(new ProbabilisticSampler(samplingRate), new RateLimitingSampler(lowerBound))
        {}

        internal GuaranteedThroughputProbabilisticSampler(ISampler probabilisticSampler, ISampler rateLimitingSampler)
        {
            _probabilisticSampler = probabilisticSampler;
            _rateLimitingSampler = rateLimitingSampler;
        }

        public void Dispose()
        {
            _probabilisticSampler.Dispose();
            _rateLimitingSampler.Dispose();
        }

        public (bool Sampled, IDictionary<string, Field> Tags) IsSampled(TraceId id, string operation)
        {
            var probabilisticSampling = _probabilisticSampler.IsSampled(id, operation);
            var rateLimitingSampling = _rateLimitingSampler.IsSampled(id, operation);

            if (probabilisticSampling.Sampled) {
                return probabilisticSampling;
            }

            return rateLimitingSampling;
        }
    }
}
