using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace C0304_ModuleA.Models
{
	public class Person : INotifyPropertyChanged
	{
		#region Properties

		private string _firstName;
		public string FirstName
		{
			get => _firstName;
			set
			{
				_firstName = value;
				OnPropertyChanged();
			}
		}

		private string _lastName;
		public string LastName
		{
			get => _lastName;
			set
			{
				_lastName = value;
				OnPropertyChanged();
			}
		}

		private int _age;
		public int Age
		{
			get => _age;
			set
			{
				_age = value;
				OnPropertyChanged();
			}
		}

		private DateTime? _lastUpdated;
		public DateTime? LastUpdated
		{
			get => _lastUpdated;
			set
			{
				_lastUpdated = value;
				OnPropertyChanged();
			}
		}

		#endregion //Properties

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion //INotifyPropertyChanged

		public override string ToString()
		{
			return $"{LastName}, {FirstName}";
		}
	}
}
