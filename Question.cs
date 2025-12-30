using System.Collections.Generic;
using Newtonsoft.Json;

namespace AwsExamSimulator
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public List<int> CorrectAnswers { get; set; } = new List<int>(); // 0-based indices (supports single or multiple answers)
        public string Explanation { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>(); // e.g., "EC2", "S3", "IAM"
        
        // Backward compatibility: handle old "CorrectAnswer" (int) format during deserialization
        [JsonProperty("CorrectAnswer")]
        private int? CorrectAnswerLegacy 
        { 
            set 
            { 
                if (value.HasValue && CorrectAnswers.Count == 0)
                {
                    CorrectAnswers.Add(value.Value);
                }
            }
        }
        
        public bool HasMultipleAnswers => CorrectAnswers.Count > 1;
        public int RequiredAnswerCount => CorrectAnswers.Count;
    }
}

