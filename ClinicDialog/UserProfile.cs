﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples
{
    using System.Collections.Generic;

    /// <summary>Contains information about a user.</summary>
    public class UserProfile
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        // The list of companies the user wants to review.
        public List<string> CompaniesToReview { get; set; } = new List<string>();

        public List<string> symptomList { get; set; } = new List<string>();
    }
}
