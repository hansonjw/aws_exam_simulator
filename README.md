# AWS Certification Practice Exam

A C# command-line application for practicing AWS certification exam questions.

## Background

This program was developed to help prepare for AWS certifications.  The program was created using Cursor and Grok and was created with the intent of push Cursor and Grok capabilities.  The author is thoroughly impressed with the program.

## Features

- Load questions from a JSON file
- Random question selection
- Support for single and multiple correct answers
- Immediate feedback with explanations
- Score tracking
- Tagged questions for topic organization
- Filter questions by tag

## Getting Started

### Prerequisites

- .NET SDK 8.0 or later
- A `questions.json` file with exam questions (a sample file is included)

### Running the Program

1. Restore dependencies:
   ```bash
   dotnet restore
   ```

2. Run the program:
   ```bash
   dotnet run
   ```

3. Optionally specify a custom questions file:
   ```bash
   dotnet run questions.json
   dotnet run questions_merged.json
   ```

4. Filter questions by tag:
   ```bash
   dotnet run --tag "Justin Researched"
   dotnet run --tag "AWS Official"
   dotnet run questions_merged.json --tag "Justin Researched"
   ```

## Questions File Format

The `questions.json` file should contain an array of question objects with the following structure:

```json
{
  "Id": 1,
  "QuestionText": "Your question here?",
  "Options": [
    "Option 1",
    "Option 2",
    "Option 3",
    "Option 4"
  ],
  "CorrectAnswers": [0],
  "Explanation": "Explanation of the correct answer",
  "Tags": ["Tag1", "Tag2"]
}
```

For questions with multiple correct answers (e.g., "Select TWO"):
```json
{
  "QuestionText": "Which services are serverless? (Select TWO.)",
  "CorrectAnswers": [0, 2],
  ...
}
```

- `Id`: Unique identifier for the question
- `QuestionText`: The question text (can include "(Select TWO.)", "(Select THREE.)", etc. for multiple answer questions)
- `Options`: Array of answer choices (typically 4 options, but can be 2-5)
- `CorrectAnswers`: Array of 0-based indices of the correct answer(s). Single answer questions have one element, multiple answer questions have multiple elements.
- `Explanation`: Explanation shown after answering
- `Tags`: Optional array of tags for categorization (e.g., "EC2", "S3", "IAM", "AWS Official", "Justin Researched")

## Adding Questions

This program has a blank questions.json file.

Edit the `questions.json` file and add your questions following the format above. The program will automatically load all questions from the file when it starts.

Please contact me if you are interested in getting some of my question files for practice.  Thanks!

## 