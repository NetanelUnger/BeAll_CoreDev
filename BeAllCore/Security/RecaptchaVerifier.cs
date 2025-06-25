using Google.Cloud.RecaptchaEnterprise.V1;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BeAllCore.Security
{
    public static class RecaptchaVerifier
    {
        public static async Task<bool> VerifyToken(string projectId, string siteKey, string token)
        {
            string credentialPath = Path.Combine(AppContext.BaseDirectory, "Credentials", "recaptcha.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

            RecaptchaEnterpriseServiceClient client = await RecaptchaEnterpriseServiceClient.CreateAsync();

            CreateAssessmentRequest request = new CreateAssessmentRequest
            {
                ParentAsProjectName = new Google.Api.Gax.ResourceNames.ProjectName(projectId),
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
