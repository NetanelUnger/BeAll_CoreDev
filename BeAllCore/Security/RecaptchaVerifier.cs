using Google.Cloud.RecaptchaEnterprise.V1;
using System.Threading.Tasks;

namespace BeAllCore.Security
{
    public static class RecaptchaVerifier
    {
        public static async Task<bool> VerifyToken(string projectId, string siteKey, string token)
        {
            RecaptchaEnterpriseServiceClient client = await RecaptchaEnterpriseServiceClient.CreateAsync();

            CreateAssessmentRequest request = new CreateAssessmentRequest
            {
                ParentAsProjectName = new ProjectName(projectId),
                Assessment = new Assessment
                {
                    Event = new Event
                    {
                        SiteKey = siteKey,
                        Token = token
                    }
                }
            };

            Assessment response = await client.CreateAssessmentAsync(request);

            return response.TokenProperties != null && response.TokenProperties.Valid;
        }
    }
}
