﻿using System;
using System.Threading;

using AventStack.ExtentReports.Gherkin.Model;

using MongoDB.Bson;

namespace AventStack.ExtentReports.Model
{
    [Serializable]
    public class Test
    {
        public string HierarchicalName { get; private set; }
        public string Description { get; set; }
        public int Level { get; internal set; }
        public int TestId { get; private set; }
        public ExceptionInfo ExceptionInfo { get; set; }
        public ObjectId ObjectId { get; set; }
        public Status Status { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        private string _name;
        private Test _parent;
        private static int _cntr;
        private GenericStructure<Log> _logContext;
        private GenericStructure<Test> _nodeContext;
        private GenericStructure<TestAttribute> _authorContext;
        private GenericStructure<TestAttribute> _categoryContext;
        private GenericStructure<ScreenCapture> _screenCaptureContext;

        public Test()
        {
            StartTime = DateTime.Now;
            EndTime = StartTime;
            Status = Status.Pass;
            Level = 0;

            TestId = Interlocked.Increment(ref _cntr);
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                HierarchicalName = Name;
            }
        }

        public Test Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
                HierarchicalName = Parent.HierarchicalName + "." + Name;
            }
        }

        public bool IsChild
        {
            get
            {
                return Level > 0;
            }
        }

        public TimeSpan RunDuration
        {
            get
            {
                return EndTime.Subtract(StartTime);
            }
        }

        public GenericStructure<Test> NodeContext()
        {
            if (_nodeContext == null)
                _nodeContext = new GenericStructure<Test>();

            return _nodeContext;
        }

        public bool HasChildren()
        {
            return NodeContext() != null && NodeContext().GetAllItems() != null && NodeContext().Count > 0;
        }

        public GenericStructure<Log> LogContext()
        {
            if (_logContext == null)
                _logContext = new GenericStructure<Log>();

            return _logContext;
        }

        public bool HasLog()
        {
            return LogContext() != null && LogContext().GetAllItems() != null && LogContext().Count > 0;
        }

        public GenericStructure<TestAttribute> CategoryContext()
        {
            if (_categoryContext == null)
                _categoryContext = new GenericStructure<TestAttribute>();

            return _categoryContext;
        }

        public bool HasException()
        {
            if (ExceptionInfo != null && ExceptionInfo.Exception != null)
            {
                return true;
            }

            return false;
        }

        public bool HasCategory()
        {
            return CategoryContext() != null && CategoryContext().GetAllItems() != null && CategoryContext().Count > 0;
        }

        public GenericStructure<TestAttribute> AuthorContext()
        {
            if (_authorContext == null)
                _authorContext = new GenericStructure<TestAttribute>();

            return _authorContext;
        }

        public bool HasAuthor()
        {
            return AuthorContext() != null && AuthorContext().GetAllItems() != null && AuthorContext().Count > 0;
        }

        public GenericStructure<ScreenCapture> ScreenCaptureContext()
        {
            if (_screenCaptureContext == null)
                _screenCaptureContext = new GenericStructure<ScreenCapture>();

            return _screenCaptureContext;
        }

        public bool HasScreenCapture()
        {
            if (_screenCaptureContext == null)
                return false;

            return _screenCaptureContext.Count > 0;
        }

        public IGherkinFormatterModel BehaviorDrivenType;

        public bool IsBehaviorDrivenType
        {
            get
            {
                return BehaviorDrivenType != null;
            }
        }

        public void End()
        {
            UpdateTestStatusRecursive(this);
            EndChildTestsRecursive(this);

            Status = Status == Status.Info ? Status.Pass : Status;
        }

        private void UpdateTestStatusRecursive(Test test)
        {
            test.LogContext().GetAllItems().ForEach(x => UpdateStatus(x.Status));

            if (test.HasChildren())
                test.NodeContext().GetAllItems().ForEach(x => UpdateTestStatusRecursive(x));
        }

        private void UpdateStatus(Status status)
        {
            int statusIndex = StatusHierarchy.GetStatusHierarchy().IndexOf(status);
            int testStatusIndex = StatusHierarchy.GetStatusHierarchy().IndexOf(Status);

            Status = statusIndex < testStatusIndex ? status : Status;
        }

        private void EndChildTestsRecursive(Test test)
        {
            test.NodeContext().GetAllItems().ForEach(x => x.End());
        }
    }
}
