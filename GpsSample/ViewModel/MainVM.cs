﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace GpsSample.ViewModel
{
    public class MainVM : ViewModelBase
    {
        #region ctor(s)

        public MainVM()
        {
            this.Methods = new List<string> {"none (stop tracking)", "gps event", "timer"};
            this._geoLocator = new Geolocator {MovementThreshold = 15, DesiredAccuracy = PositionAccuracy.Default};
            this._timer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 2)};
            this._task = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
            this.Log = new ObservableCollection<string>();
        }

        #endregion

        #region logging

        private void LogAnUpdate(string lat, string lon)
        {
            Log.Add(DateTime.Now.ToString("HH:mm:ss") + ": " + lat + ", " + lon);
        }

        #endregion

        #region fields & props

        private readonly Geolocator _geoLocator;
        private readonly DispatcherTimer _timer;
        private readonly TaskFactory _task;


        private string _longitude;

        public string Longitude
        {
            get { return _longitude; }
            set
            {
                _longitude = value;
                RaisePropertyChanged();
            }
        }

        private string _latitude;

        public string Latitude
        {
            get { return _latitude; }
            set
            {
                _latitude = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _log;

        public ObservableCollection<string> Log
        {
            get { return _log; }
            set
            {
                _log = value;
                RaisePropertyChanged();
            }
        }


        public string SelectedMethod { get; set; }

        public List<string> Methods { get; set; }

        #endregion

        #region commands

        public ICommand MethodChangedCommand
        {
            get { return new RelayCommand(MethodChanged); }
        }

        private void MethodChanged()
        {
            switch (SelectedMethod)
            {
                case "none (stop tracking)":
                    StopContinuousTrackingGeo();
                    StopContinuousTrackingTimer();
                    break;
                case "gps event":
                    StartContinuousTrackingGeo();
                    StopContinuousTrackingTimer();
                    break;
                case "timer":
                    StopContinuousTrackingGeo();
                    StartContinuousTrackingTimer();
                    break;
            }
        }

        public ICommand ClearLogCommand
        {
            get { return new RelayCommand(ClearLog); }
        }

        private void ClearLog()
        {
            Log.Clear();
        }

        #endregion

        #region geolocator logic

        public void StartContinuousTrackingGeo()
        {
            _geoLocator.PositionChanged += GeoLocatorPositionChanged;
        }

        public void StopContinuousTrackingGeo()
        {
            _geoLocator.PositionChanged -= GeoLocatorPositionChanged;
        }

        private async void GeoLocatorPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await _task.StartNew(() =>
            {
                Latitude = "Latitude: " + args.Position.Coordinate.Latitude;
                Longitude = "Longitude: " + args.Position.Coordinate.Longitude;
                LogAnUpdate(Latitude, Longitude);
            });
        }

        #endregion

        #region timer events

        public void StartContinuousTrackingTimer()
        {
            _timer.Tick += UpdatePosition;
            _timer.Start();
        }

        private void StopContinuousTrackingTimer()
        {
            _timer.Stop();
            _timer.Tick -= UpdatePosition;
        }

        private async void UpdatePosition(object sender, object e)
        {
            var location = await _geoLocator.GetGeopositionAsync();

            if (location != null)
            {
                Latitude = "Latitude: " + location.Coordinate.Latitude;
                Longitude = "Longitude: " + location.Coordinate.Longitude;
                LogAnUpdate(Latitude, Longitude);
            }
        }

        #endregion
    }
}