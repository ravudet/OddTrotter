namespace OddTrotter.Odata.v4_01.ConventionContext
{
    using System.Threading.Tasks;

    internal interface IConventionContext
    {
        Task<ConventionResponse> Evaluate(ConventionRequest request);
    }
}
