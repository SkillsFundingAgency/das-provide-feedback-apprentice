using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
using Polly;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    public class ApprenticeSurveyInvitesRepository : IStoreApprenticeSurveyDetails
    {
        private const int _commandTimeoutSeconds = 120;
        private readonly IDbConnection _dbConnection;

        public ApprenticeSurveyInvitesRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<ApprenticeSurveyInvite>> GetApprenticeSurveyInvitesAsync(int batchSize)
        {
            return await _dbConnection.QueryAsync<ApprenticeSurveyInvite>(sql: $@"
                                        SELECT TOP {batchSize} * 
                                        FROM ApprenticeSurveyInvites
                                        WHERE SentDate IS NULL", param: null, transaction: null, commandTimeout: _commandTimeoutSeconds);
        }

        public async Task SetApprenticeSurveySentAsync(long mobileNumber, string surveyCode)
        {
            var now = DateTime.Now;
            var sql = $@"
                        UPDATE ApprenticeSurveyInvites
                        SET EmailSentDate = @{nameof(now)}
                        WHERE MobileNumber = @{nameof(mobileNumber)}
                        AND SurveyCode = @{nameof(surveyCode)}";

            await ExecuteUpdateAsync(sql, new { now, mobileNumber, surveyCode });
        }

        private async Task ExecuteUpdateAsync(string sql, object param)
        {
            var policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(3));

            await policy.ExecuteAsync(() =>
            {
                return _dbConnection.QueryAsync(sql: sql, param: param, transaction: null, commandTimeout: _commandTimeoutSeconds);
            });
        }
    }
}
