﻿using System;
using System.Collections.Generic;
using Automatica.Core.Model;
using MessagePack;
using Newtonsoft.Json;

namespace Automatica.Core.EF.Models
{
    public enum RuleInterfaceParameterDataType
    {
        NoParameter,
        Integer,
        Double,
        Text,
        Timer
    }

    public class TimerPropertyData : TypedObject
    {
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public IList<DayOfWeek> EnabledDays { get; set; }
    }

    public class RuleInterfaceTemplate : TypedObject
    {
        public Guid ObjId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid This2RuleTemplate { get; set; }
        public long This2RuleInterfaceDirection { get; set; }
        public int MaxLinks { get; set; }

        public RuleInterfaceParameterDataType ParameterDataType { get; set; }

        public int SortOrder { get; set; }

        public string DefaultValue { get; set; }

        public bool IsLinkableParameter { get; set; }

        public RuleInterfaceDirection This2RuleInterfaceDirectionNavigation { get; set; }

        [JsonIgnore, IgnoreMember]
        public RuleTemplate This2RuleTemplateNavigation { get; set; }
        
    }
}
