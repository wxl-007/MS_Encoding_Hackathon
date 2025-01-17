﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.BotBuilderSamples
{
    public class TopLevelDialog : ComponentDialog
    {
        #region initial 
        // Define a "done" response for the company selection prompt.
        private const string DoneOption = "done";

        // Define value names for values tracked inside the dialogs.
        private const string UserInfo = "value-userInfo";

        List<string> specificSymptoms = new List<string> { "headache", "sore throat", "fever" };
  
        public TopLevelDialog()
            : base(nameof(TopLevelDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            
            AddDialog(new ReviewSelectionDialog());

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NameStepAsync,
                AgeStepAsync,
                PhoneNumberStepAsync,
                EmailStepAsync,
                GenderStepAsync,
                StartSelectionStepAsync,
                AcknowledgementStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion

        #region ask name 
        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create an object in which to collect the user's information within the dialog.
            stepContext.Values[UserInfo] = new UserProfile();

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") };

            // Ask the user to enter their name.
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }
        #endregion

        #region ask age
        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Set the user's name to what they entered in response to the name prompt.
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Name = (string)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter your age.") };

            // Ask the user to enter their age.
            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }
        #endregion

        #region ask phone number
        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Age = (int)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter your phone number.") };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }
        #endregion

        #region ask email
        private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.PhoneNumber = (string)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter your Email address.") };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }
        #endregion

        #region ask gender
        private async Task<DialogTurnResult> GenderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Email = (string)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter your Gender: Female or Male.") };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }
        #endregion


        #region describe your symptoms 
        private async Task<DialogTurnResult> StartDescribeSymptoms(WaterfallStepContext stepContext, CancellationToken cancellationToken) {

            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Gender = (string)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("What are your symptoms?") };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }
        #endregion



        private bool[] CheckSpecificSymptom(string symptomList) {
            if (string.IsNullOrEmpty(symptomList) || string.IsNullOrWhiteSpace(symptomList)) return null;
            bool[] result = new bool[specificSymptoms.Count];
            for (int i = 0; i < specificSymptoms.Count; i++)
            {
                result[i] = symptomList.Contains(specificSymptoms[i]);
            }
            return result;
        }

        #region selection 
        private async Task<DialogTurnResult> StartSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Set the user's age to what they entered in response to the age prompt.
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            string symptomSentence = (string)stepContext.Result;
            bool[] followingQuestions =  CheckSpecificSymptom(symptomSentence);

            if (followingQuestions == null)
            {
                await stepContext.Context.SendActivityAsync(
                       MessageFactory.Text("We are saving your basic info."),
                       cancellationToken);
                return await stepContext.NextAsync(new List<string>(), cancellationToken);

            }
            else {
                return await stepContext.BeginDialogAsync(nameof(ReviewSelectionDialog), null, cancellationToken);

            }

            //if (userProfile.Age < 16)
            //{
            //    // If they are too young, skip the review selection dialog, and pass an empty list to the next step.
            //    await stepContext.Context.SendActivityAsync(
            //        MessageFactory.Text("You are to young, just call your mom to take you back."),
            //        cancellationToken);
            //    return await stepContext.NextAsync(new List<string>(), cancellationToken);
            //}
            //else
            //{
            //    // Otherwise, start the review selection dialog.
            //    return await stepContext.BeginDialogAsync(nameof(ReviewSelectionDialog), null, cancellationToken);
            //}
        }
        #endregion

        #region acknowledgement
        private async Task<DialogTurnResult> AcknowledgementStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Set the user's company selection to what they entered in the review-selection dialog.
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.CompaniesToReview = stepContext.Result as List<string> ?? new List<string>();

            // Thank them for participating.
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Thanks for coming to clinic, {((UserProfile)stepContext.Values[UserInfo]).Name}.Generating your document.. Please be patient. "),
                cancellationToken);

            // Exit the dialog, returning the collected user information.
            return await stepContext.EndDialogAsync(stepContext.Values[UserInfo], cancellationToken);
        }
        #endregion
    }
}
