using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.CommandLine;
using Newtonsoft.Json;

namespace AwsCertPractice
{
    class Program
    {
        private static List<Question> questions = new List<Question>();
        private static int correctAnswers = 0;
        private static int totalQuestions = 0;
        private static List<(Question question, List<int> userAnswers)> incorrectQuestions = new List<(Question, List<int>)>();

        static async Task<int> Main(string[] args)
        {
            var questionsFileArgument = new Argument<string>(
                name: "file",
                description: "Path to the questions JSON file",
                getDefaultValue: () => "questions.json");

            var tagOption = new Option<string?>(
                aliases: new[] { "--tag", "-t" },
                description: "Filter questions by tag");

            var rootCommand = new RootCommand("AWS Certification Practice Exam")
            {
                questionsFileArgument,
                tagOption
            };

            rootCommand.SetHandler((questionsFile, filterTag) =>
            {
                RunExam(questionsFile, filterTag);
            }, questionsFileArgument, tagOption);

            return await rootCommand.InvokeAsync(args);
        }

        static void RunExam(string questionsFile, string? filterTag)
        {
            Console.WriteLine("AWS Certification Practice Exam");
            Console.WriteLine("===============================\n");

            if (!LoadQuestions(questionsFile))
            {
                Console.WriteLine($"Error: Could not load questions from {questionsFile}");
                Console.WriteLine("Please ensure the file exists and is valid JSON.");
                return;
            }

            // Filter by tag if specified
            if (!string.IsNullOrEmpty(filterTag))
            {
                FilterQuestionsByTag(filterTag);
                if (questions.Count == 0)
                {
                    Console.WriteLine($"No questions found with tag '{filterTag}'");
                    return;
                }
                Console.WriteLine($"Filtered to {questions.Count} question(s) with tag '{filterTag}'\n");
            }
            else
            {
                Console.WriteLine($"Loaded {questions.Count} question(s)\n");
            }

            // Ask how many questions to practice
            int numQuestions = AskNumberOfQuestions();
            
            // Shuffle and select questions
            var selectedQuestions = SelectRandomQuestions(numQuestions);

            // Reset exam state
            ResetExamState();

            // Run the exam
            RunExamQuestions(selectedQuestions);

            // Show results
            ShowResults();

            // Show incorrect questions
            ShowIncorrectQuestions();
        }

        static bool LoadQuestions(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    Console.WriteLine($"Questions file '{filename}' not found. Creating a sample file...");
                    CreateSampleQuestionsFile(filename);
                    return false;
                }

                string json = File.ReadAllText(filename);
                questions = JsonConvert.DeserializeObject<List<Question>>(json) ?? new List<Question>();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading questions: {ex.Message}");
                return false;
            }
        }

        static void CreateSampleQuestionsFile(string filename)
        {
            var sampleQuestions = new List<Question>
            {
                new Question
                {
                    Id = 1,
                    QuestionText = "Which AWS service provides a fully managed NoSQL database?",
                    Options = new List<string> { "Amazon RDS", "Amazon DynamoDB", "Amazon Redshift", "Amazon ElastiCache" },
                    CorrectAnswers = new List<int> { 1 },
                    Explanation = "Amazon DynamoDB is a fully managed NoSQL database service that provides fast and predictable performance with seamless scalability.",
                    Tags = new List<string> { "DynamoDB", "Database" }
                },
                new Question
                {
                    Id = 2,
                    QuestionText = "What is the maximum size of an object in Amazon S3?",
                    Options = new List<string> { "1 GB", "5 GB", "5 TB", "Unlimited" },
                    CorrectAnswers = new List<int> { 2 },
                    Explanation = "The maximum size of a single object in Amazon S3 is 5 TB. However, you must use multipart upload for objects larger than 5 GB.",
                    Tags = new List<string> { "S3", "Storage" }
                }
            };

            string json = JsonConvert.SerializeObject(sampleQuestions, Formatting.Indented);
            File.WriteAllText(filename, json);
            Console.WriteLine($"Sample questions file created: {filename}");
        }

        static int AskNumberOfQuestions()
        {
            Console.Write($"How many questions would you like to practice? (1-{questions.Count}): ");
            string? input = Console.ReadLine();
            
            if (int.TryParse(input, out int num) && num > 0 && num <= questions.Count)
            {
                return num;
            }
            
            Console.WriteLine($"Invalid input. Using {Math.Min(10, questions.Count)} questions.");
            return Math.Min(10, questions.Count);
        }

        static void FilterQuestionsByTag(string tag)
        {
            questions = questions.Where(q => q.Tags != null && q.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();
        }

        static List<Question> SelectRandomQuestions(int count)
        {
            var random = new Random();
            return questions.OrderBy(x => random.Next()).Take(count).ToList();
        }

        static void ResetExamState()
        {
            correctAnswers = 0;
            totalQuestions = 0;
            incorrectQuestions.Clear();
        }

        static void RunExamQuestions(List<Question> selectedQuestions)
        {
            totalQuestions = selectedQuestions.Count;
            correctAnswers = 0;
            incorrectQuestions.Clear();

            for (int i = 0; i < selectedQuestions.Count; i++)
            {
                Question q = selectedQuestions[i];
                Console.WriteLine($"\nQuestion {i + 1} of {selectedQuestions.Count}");
                Console.WriteLine(new string('=', 50));
                Console.WriteLine(q.QuestionText);
                Console.WriteLine();

                // Display options
                for (int j = 0; j < q.Options.Count; j++)
                {
                    Console.WriteLine($"{j + 1}. {q.Options[j]}");
                }

                // Check if question requires multiple answers
                bool requiresMultiple = q.HasMultipleAnswers || 
                                       q.QuestionText.Contains("(Select TWO)", StringComparison.OrdinalIgnoreCase) ||
                                       q.QuestionText.Contains("(Select THREE)", StringComparison.OrdinalIgnoreCase) ||
                                       q.QuestionText.Contains("(Select MULTIPLE)", StringComparison.OrdinalIgnoreCase);
                
                int requiredCount = q.HasMultipleAnswers ? q.CorrectAnswers.Count : 1;

                // Get user answer(s)
                if (requiresMultiple)
                {
                    Console.Write($"\nEnter {requiredCount} answers separated by commas (e.g., 1,3): ");
                }
                else
                {
                    Console.Write($"\nEnter your answer (1-{q.Options.Count}): ");
                }
                
                string? answerInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(answerInput))
                {
                    Console.WriteLine("\nInvalid answer.");
                    Console.WriteLine($"The correct answer(s) are: {GetCorrectAnswersDisplay(q)}");
                    Console.WriteLine($"\nExplanation: {q.Explanation}");
                    incorrectQuestions.Add((q, new List<int>()));
                }
                else if (requiresMultiple)
                {
                    // Parse multiple answers
                    var userAnswers = new HashSet<int>();
                    string[] parts = answerInput.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    
                    bool validInput = true;
                    foreach (string part in parts)
                    {
                        if (int.TryParse(part.Trim(), out int userAnswer) && userAnswer >= 1 && userAnswer <= q.Options.Count)
                        {
                            userAnswers.Add(userAnswer - 1); // Convert to 0-based index
                        }
                        else
                        {
                            validInput = false;
                            break;
                        }
                    }

                    if (validInput && userAnswers.Count == requiredCount)
                    {
                        // Check if all correct answers were selected
                        var correctAnswersSet = new HashSet<int>(q.CorrectAnswers);
                        if (userAnswers.SetEquals(correctAnswersSet))
                        {
                            Console.WriteLine("\n✓ Correct!");
                            correctAnswers++;
                        }
                        else
                        {
                            Console.WriteLine("\n✗ Incorrect.");
                            Console.WriteLine($"The correct answers are: {GetCorrectAnswersDisplay(q)}");
                            incorrectQuestions.Add((q, userAnswers.ToList()));
                        }
                    }
                    else
                    {
                        Console.WriteLine($"\nInvalid input. Please enter exactly {requiredCount} valid answers separated by commas.");
                        Console.WriteLine($"The correct answers are: {GetCorrectAnswersDisplay(q)}");
                        incorrectQuestions.Add((q, userAnswers.ToList()));
                    }
                }
                else
                {
                    // Single answer question
                    if (int.TryParse(answerInput, out int userAnswer) && userAnswer >= 1 && userAnswer <= q.Options.Count)
                    {
                        int answerIndex = userAnswer - 1; // Convert to 0-based index
                        
                        if (q.CorrectAnswers.Contains(answerIndex))
                        {
                            Console.WriteLine("\n✓ Correct!");
                            correctAnswers++;
                        }
                        else
                        {
                            Console.WriteLine("\n✗ Incorrect.");
                            Console.WriteLine($"The correct answer is: {GetCorrectAnswersDisplay(q)}");
                            incorrectQuestions.Add((q, new List<int> { answerIndex }));
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nInvalid answer.");
                        Console.WriteLine($"The correct answer is: {GetCorrectAnswersDisplay(q)}");
                        Console.WriteLine($"\nExplanation: {q.Explanation}");
                        incorrectQuestions.Add((q, new List<int>()));
                    }
                }

                Console.WriteLine($"\nExplanation: {q.Explanation}");

                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }

        static string GetCorrectAnswersDisplay(Question q)
        {
            if (q.CorrectAnswers.Count == 0)
                return "N/A";
            
            var answerNumbers = q.CorrectAnswers.Select(idx => (idx + 1).ToString()).ToArray();
            return string.Join(", ", answerNumbers);
        }

        static void ShowResults()
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("EXAM RESULTS");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Total Questions: {totalQuestions}");
            Console.WriteLine($"Correct Answers: {correctAnswers}");
            Console.WriteLine($"Incorrect Answers: {totalQuestions - correctAnswers}");
            double percentage = totalQuestions > 0 ? (double)correctAnswers / totalQuestions * 100 : 0;
            Console.WriteLine($"Score: {percentage:F1}%");
            Console.WriteLine(new string('=', 50));
        }

        static void ShowIncorrectQuestions()
        {
            if (incorrectQuestions.Count > 0)
            {
                Console.WriteLine("\n" + new string('=', 50));
                Console.WriteLine($"QUESTIONS ANSWERED INCORRECTLY ({incorrectQuestions.Count})");
                Console.WriteLine(new string('=', 50));
                
                for (int i = 0; i < incorrectQuestions.Count; i++)
                {
                    var (q, userAnswers) = incorrectQuestions[i];
                    Console.WriteLine($"\n{i + 1}. Question ID: {q.Id}");
                    Console.WriteLine($"   {q.QuestionText}");
                    
                    // Show user's submitted answer(s)
                    if (userAnswers.Count > 0)
                    {
                        var userAnswerNumbers = userAnswers.Select(idx => (idx + 1).ToString()).ToArray();
                        if (userAnswers.Count == 1)
                        {
                            Console.WriteLine($"   Your Answer: {userAnswerNumbers[0]}");
                        }
                        else
                        {
                            Console.WriteLine($"   Your Answers: {string.Join(", ", userAnswerNumbers)}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"   Your Answer: (no answer or invalid input)");
                    }
                    
                    // Show correct answer(s)
                    if (q.CorrectAnswers.Count > 0)
                    {
                        var answerNumbers = q.CorrectAnswers.Select(idx => (idx + 1).ToString()).ToArray();
                        if (q.CorrectAnswers.Count == 1)
                        {
                            Console.WriteLine($"   Correct Answer: {answerNumbers[0]}");
                        }
                        else
                        {
                            Console.WriteLine($"   Correct Answers: {string.Join(", ", answerNumbers)}");
                        }
                    }
                    
                    // Show explanation
                    if (!string.IsNullOrWhiteSpace(q.Explanation))
                    {
                        Console.WriteLine($"   Explanation: {q.Explanation}");
                    }
                }
                
                Console.WriteLine("\n" + new string('=', 50));
            }
            else
            {
                Console.WriteLine("\n" + new string('=', 50));
                Console.WriteLine("Perfect! You answered all questions correctly!");
                Console.WriteLine(new string('=', 50));
            }
        }
    }
}

