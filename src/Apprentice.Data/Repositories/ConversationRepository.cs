using System.Data;
using System.Threading.Tasks;
using Dapper;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private const int _commandTimeoutSeconds = 120;
        private readonly IDbConnection _dbConnection;

        public ConversationRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public Task<Conversation> Get(string id)
        {
            return _dbConnection.QueryFirstOrDefaultAsync<Conversation>(sql: $@"
                                        SELECT Id, UserId, ActivityId, TurnId 
                                        FROM Conversations
                                        WHERE Id = @{nameof(id)}", param: new { id }, commandTimeout: _commandTimeoutSeconds);
        }

        public Task Save(Conversation conversation)
        {
            var sql = $@"
                        MERGE Conversations AS [Target]
                        USING (
                        SELECT 
                            @{nameof(conversation.Id)} AS Id, 
                            @{nameof(conversation.UserId)} AS UserId, 
                            @{nameof(conversation.ActivityId)} AS ActivityId, 
                            @{nameof(conversation.TurnId)} AS TurnId
                            ) AS [Source] 
                        ON [Target].Id = [Source].Id
                        WHEN MATCHED THEN 
                            UPDATE SET [Target].ActivityId = [Source].ActivityId, [Target].TurnId = [Source].TurnId
                        WHEN NOT MATCHED THEN 
                            INSERT (Id, UserId, ActivityId, TurnId ) VALUES ([Source].Id, [Source].UserId, [Source].ActivityId, [Source].TurnId);";

            return _dbConnection.ExecuteAsync(sql, param: new { conversation.Id, conversation.UserId, conversation.ActivityId, conversation.TurnId });
        }
    }
}
