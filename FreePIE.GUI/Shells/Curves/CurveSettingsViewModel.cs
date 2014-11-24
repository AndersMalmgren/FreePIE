﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using FreePIE.Core.Persistence;
using FreePIE.GUI.Events;
using FreePIE.GUI.Result;
using FreePIE.GUI.Views.Curves;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Shells.Curves
{
    public class CurveSettingsViewModel : ShellPresentationModel, Core.Common.Events.IHandle<DeleteCurveEvent>
    {
        private readonly ISettingsManager settingsManager;
        private readonly Func<CurveViewModel> curveModelFactory;

        public CurveSettingsViewModel(IResultFactory resultFactory, ISettingsManager settingsManager, Func<CurveViewModel> curveModelFactory, IEventAggregator eventAggregator) : base(resultFactory)
        {
            this.settingsManager = settingsManager;
            this.curveModelFactory = curveModelFactory;
            DisplayName = "Curve settings";
            CreateCurvesModel();
            eventAggregator.Subscribe(this);
        }

        public void Handle(DeleteCurveEvent message)
        {
            settingsManager.Settings.RemoveCurve(message.CurveViewModel.Curve);
            Curves.Remove(message.CurveViewModel);
        }

        private void CreateCurvesModel()
        {
            Curves = new BindableCollection<CurveViewModel>(settingsManager.Settings.Curves.Select(c => curveModelFactory().Configure(c)));
        }

        public IEnumerable<IResult> AddCurve()
        {
            var result = Result.ShowDialog<NewCurveViewModel>();
            yield return result;

            if (result.Model.NewCurve != null)
            {
                settingsManager.Settings.AddNewCurve(result.Model.NewCurve);
                CreateCurvesModel();                
            }
        }

        private BindableCollection<CurveViewModel> curves;
        public BindableCollection<CurveViewModel> Curves
        {
            get { return curves; }
            set 
            {
                curves = value;
                NotifyOfPropertyChange(() => Curves);
            }
        }
    }
}
