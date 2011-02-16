﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace server
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="MedicalIS")]
	public partial class MedicalISDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertPatient(Patient instance);
    partial void UpdatePatient(Patient instance);
    partial void DeletePatient(Patient instance);
    partial void InsertStudy(Study instance);
    partial void UpdateStudy(Study instance);
    partial void DeleteStudy(Study instance);
    #endregion
		
		public MedicalISDataContext() : 
				base(global::server.Properties.Settings.Default.MedicalISConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public MedicalISDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MedicalISDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MedicalISDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MedicalISDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<Patient> Patients
		{
			get
			{
				return this.GetTable<Patient>();
			}
		}
		
		public System.Data.Linq.Table<Study> Studies
		{
			get
			{
				return this.GetTable<Study>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Patient")]
	public partial class Patient : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private System.Guid _PatientId;
		
		private string _ExternalPatientID;
		
		private string _FirstName;
		
		private string _LastName;
		
		private System.Nullable<System.DateTime> _BirthDateTime;
		
		private string _Gender;
		
		private string _RootStoragePath;
		
		private System.Nullable<System.DateTime> _CreatedDateTime;
		
		private System.Nullable<System.Guid> _SenderId;
		
		private string _Species;
		
		private System.Nullable<bool> _Castrated;
		
		private System.Nullable<System.Guid> _OwnerId;
		
		private string _Title;
		
		private string _Project;
		
		private string _Breed;
		
		private string _BreedExternalIdType;
		
		private string _BreedExternalId;
		
		private string _BreedName;
		
		private string _BreedBookNumber;
		
		private System.Nullable<bool> _BirthdateNotExact;
		
		private System.Xml.Linq.XElement _Properties;
		
		private EntitySet<Study> _Studies;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnPatientIdChanging(System.Guid value);
    partial void OnPatientIdChanged();
    partial void OnExternalPatientIDChanging(string value);
    partial void OnExternalPatientIDChanged();
    partial void OnFirstNameChanging(string value);
    partial void OnFirstNameChanged();
    partial void OnLastNameChanging(string value);
    partial void OnLastNameChanged();
    partial void OnBirthDateTimeChanging(System.Nullable<System.DateTime> value);
    partial void OnBirthDateTimeChanged();
    partial void OnGenderChanging(string value);
    partial void OnGenderChanged();
    partial void OnRootStoragePathChanging(string value);
    partial void OnRootStoragePathChanged();
    partial void OnCreatedDateTimeChanging(System.Nullable<System.DateTime> value);
    partial void OnCreatedDateTimeChanged();
    partial void OnSenderIdChanging(System.Nullable<System.Guid> value);
    partial void OnSenderIdChanged();
    partial void OnSpeciesChanging(string value);
    partial void OnSpeciesChanged();
    partial void OnCastratedChanging(System.Nullable<bool> value);
    partial void OnCastratedChanged();
    partial void OnOwnerIdChanging(System.Nullable<System.Guid> value);
    partial void OnOwnerIdChanged();
    partial void OnTitleChanging(string value);
    partial void OnTitleChanged();
    partial void OnProjectChanging(string value);
    partial void OnProjectChanged();
    partial void OnBreedChanging(string value);
    partial void OnBreedChanged();
    partial void OnBreedExternalIdTypeChanging(string value);
    partial void OnBreedExternalIdTypeChanged();
    partial void OnBreedExternalIdChanging(string value);
    partial void OnBreedExternalIdChanged();
    partial void OnBreedNameChanging(string value);
    partial void OnBreedNameChanged();
    partial void OnBreedBookNumberChanging(string value);
    partial void OnBreedBookNumberChanged();
    partial void OnBirthdateNotExactChanging(System.Nullable<bool> value);
    partial void OnBirthdateNotExactChanged();
    partial void OnPropertiesChanging(System.Xml.Linq.XElement value);
    partial void OnPropertiesChanged();
    #endregion
		
		public Patient()
		{
			this._Studies = new EntitySet<Study>(new Action<Study>(this.attach_Studies), new Action<Study>(this.detach_Studies));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PatientId", DbType="UniqueIdentifier NOT NULL", IsPrimaryKey=true)]
		public System.Guid PatientId
		{
			get
			{
				return this._PatientId;
			}
			set
			{
				if ((this._PatientId != value))
				{
					this.OnPatientIdChanging(value);
					this.SendPropertyChanging();
					this._PatientId = value;
					this.SendPropertyChanged("PatientId");
					this.OnPatientIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ExternalPatientID", DbType="NVarChar(20)")]
		public string ExternalPatientID
		{
			get
			{
				return this._ExternalPatientID;
			}
			set
			{
				if ((this._ExternalPatientID != value))
				{
					this.OnExternalPatientIDChanging(value);
					this.SendPropertyChanging();
					this._ExternalPatientID = value;
					this.SendPropertyChanged("ExternalPatientID");
					this.OnExternalPatientIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FirstName", DbType="NVarChar(50)")]
		public string FirstName
		{
			get
			{
				return this._FirstName;
			}
			set
			{
				if ((this._FirstName != value))
				{
					this.OnFirstNameChanging(value);
					this.SendPropertyChanging();
					this._FirstName = value;
					this.SendPropertyChanged("FirstName");
					this.OnFirstNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastName", DbType="NVarChar(50)")]
		public string LastName
		{
			get
			{
				return this._LastName;
			}
			set
			{
				if ((this._LastName != value))
				{
					this.OnLastNameChanging(value);
					this.SendPropertyChanging();
					this._LastName = value;
					this.SendPropertyChanged("LastName");
					this.OnLastNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_BirthDateTime", DbType="DateTime")]
		public System.Nullable<System.DateTime> BirthDateTime
		{
			get
			{
				return this._BirthDateTime;
			}
			set
			{
				if ((this._BirthDateTime != value))
				{
					this.OnBirthDateTimeChanging(value);
					this.SendPropertyChanging();
					this._BirthDateTime = value;
					this.SendPropertyChanged("BirthDateTime");
					this.OnBirthDateTimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Gender", DbType="NVarChar(20)")]
		public string Gender
		{
			get
			{
				return this._Gender;
			}
			set
			{
				if ((this._Gender != value))
				{
					this.OnGenderChanging(value);
					this.SendPropertyChanging();
					this._Gender = value;
					this.SendPropertyChanged("Gender");
					this.OnGenderChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_RootStoragePath", DbType="NVarChar(512)")]
		public string RootStoragePath
		{
			get
			{
				return this._RootStoragePath;
			}
			set
			{
				if ((this._RootStoragePath != value))
				{
					this.OnRootStoragePathChanging(value);
					this.SendPropertyChanging();
					this._RootStoragePath = value;
					this.SendPropertyChanged("RootStoragePath");
					this.OnRootStoragePathChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CreatedDateTime", DbType="DateTime")]
		public System.Nullable<System.DateTime> CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				if ((this._CreatedDateTime != value))
				{
					this.OnCreatedDateTimeChanging(value);
					this.SendPropertyChanging();
					this._CreatedDateTime = value;
					this.SendPropertyChanged("CreatedDateTime");
					this.OnCreatedDateTimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SenderId", DbType="UniqueIdentifier")]
		public System.Nullable<System.Guid> SenderId
		{
			get
			{
				return this._SenderId;
			}
			set
			{
				if ((this._SenderId != value))
				{
					this.OnSenderIdChanging(value);
					this.SendPropertyChanging();
					this._SenderId = value;
					this.SendPropertyChanged("SenderId");
					this.OnSenderIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Species", DbType="NVarChar(50)")]
		public string Species
		{
			get
			{
				return this._Species;
			}
			set
			{
				if ((this._Species != value))
				{
					this.OnSpeciesChanging(value);
					this.SendPropertyChanging();
					this._Species = value;
					this.SendPropertyChanged("Species");
					this.OnSpeciesChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Castrated", DbType="Bit")]
		public System.Nullable<bool> Castrated
		{
			get
			{
				return this._Castrated;
			}
			set
			{
				if ((this._Castrated != value))
				{
					this.OnCastratedChanging(value);
					this.SendPropertyChanging();
					this._Castrated = value;
					this.SendPropertyChanged("Castrated");
					this.OnCastratedChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_OwnerId", DbType="UniqueIdentifier")]
		public System.Nullable<System.Guid> OwnerId
		{
			get
			{
				return this._OwnerId;
			}
			set
			{
				if ((this._OwnerId != value))
				{
					this.OnOwnerIdChanging(value);
					this.SendPropertyChanging();
					this._OwnerId = value;
					this.SendPropertyChanged("OwnerId");
					this.OnOwnerIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Title", DbType="NVarChar(50)")]
		public string Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				if ((this._Title != value))
				{
					this.OnTitleChanging(value);
					this.SendPropertyChanging();
					this._Title = value;
					this.SendPropertyChanged("Title");
					this.OnTitleChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Project", DbType="NVarChar(50)")]
		public string Project
		{
			get
			{
				return this._Project;
			}
			set
			{
				if ((this._Project != value))
				{
					this.OnProjectChanging(value);
					this.SendPropertyChanging();
					this._Project = value;
					this.SendPropertyChanged("Project");
					this.OnProjectChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Breed", DbType="NVarChar(50)")]
		public string Breed
		{
			get
			{
				return this._Breed;
			}
			set
			{
				if ((this._Breed != value))
				{
					this.OnBreedChanging(value);
					this.SendPropertyChanging();
					this._Breed = value;
					this.SendPropertyChanged("Breed");
					this.OnBreedChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_BreedExternalIdType", DbType="NVarChar(50)")]
		public string BreedExternalIdType
		{
			get
			{
				return this._BreedExternalIdType;
			}
			set
			{
				if ((this._BreedExternalIdType != value))
				{
					this.OnBreedExternalIdTypeChanging(value);
					this.SendPropertyChanging();
					this._BreedExternalIdType = value;
					this.SendPropertyChanged("BreedExternalIdType");
					this.OnBreedExternalIdTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_BreedExternalId", DbType="NVarChar(50)")]
		public string BreedExternalId
		{
			get
			{
				return this._BreedExternalId;
			}
			set
			{
				if ((this._BreedExternalId != value))
				{
					this.OnBreedExternalIdChanging(value);
					this.SendPropertyChanging();
					this._BreedExternalId = value;
					this.SendPropertyChanged("BreedExternalId");
					this.OnBreedExternalIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_BreedName", DbType="NVarChar(100)")]
		public string BreedName
		{
			get
			{
				return this._BreedName;
			}
			set
			{
				if ((this._BreedName != value))
				{
					this.OnBreedNameChanging(value);
					this.SendPropertyChanging();
					this._BreedName = value;
					this.SendPropertyChanged("BreedName");
					this.OnBreedNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_BreedBookNumber", DbType="NVarChar(50)")]
		public string BreedBookNumber
		{
			get
			{
				return this._BreedBookNumber;
			}
			set
			{
				if ((this._BreedBookNumber != value))
				{
					this.OnBreedBookNumberChanging(value);
					this.SendPropertyChanging();
					this._BreedBookNumber = value;
					this.SendPropertyChanged("BreedBookNumber");
					this.OnBreedBookNumberChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_BirthdateNotExact", DbType="Bit")]
		public System.Nullable<bool> BirthdateNotExact
		{
			get
			{
				return this._BirthdateNotExact;
			}
			set
			{
				if ((this._BirthdateNotExact != value))
				{
					this.OnBirthdateNotExactChanging(value);
					this.SendPropertyChanging();
					this._BirthdateNotExact = value;
					this.SendPropertyChanged("BirthdateNotExact");
					this.OnBirthdateNotExactChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Properties", DbType="Xml", UpdateCheck=UpdateCheck.Never)]
		public System.Xml.Linq.XElement Properties
		{
			get
			{
				return this._Properties;
			}
			set
			{
				if ((this._Properties != value))
				{
					this.OnPropertiesChanging(value);
					this.SendPropertyChanging();
					this._Properties = value;
					this.SendPropertyChanged("Properties");
					this.OnPropertiesChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Patient_Study", Storage="_Studies", ThisKey="PatientId", OtherKey="PatientId")]
		public EntitySet<Study> Studies
		{
			get
			{
				return this._Studies;
			}
			set
			{
				this._Studies.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Studies(Study entity)
		{
			this.SendPropertyChanging();
			entity.Patient = this;
		}
		
		private void detach_Studies(Study entity)
		{
			this.SendPropertyChanging();
			entity.Patient = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Study")]
	public partial class Study : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private System.Guid _StudyId;
		
		private string _ExternalStudyID;
		
		private string _Description;
		
		private System.Nullable<System.DateTime> _PerformedDateTime;
		
		private System.Nullable<System.Guid> _PatientId;
		
		private System.Nullable<int> _Index;
		
		private string _StudyInstanceUid;
		
		private string _RootStoragePath;
		
		private System.Nullable<System.DateTime> _CreatedDateTime;
		
		private string _Comment;
		
		private string _ModalityAggregation;
		
		private System.Nullable<System.Guid> _SenderId;
		
		private string _AccessionNumber;
		
		private string _OtherPatientId;
		
		private string _NameOfPhysiciansReadingStudy;
		
		private string _Veterinarian;
		
		private string _Technician;
		
		private string _ReferringPhysiciansName;
		
		private System.Nullable<System.Guid> _AssignedDocumentCollectionId;
		
		private System.Xml.Linq.XElement _Properties;
		
		private System.Nullable<System.DateTime> _LastChangedDateTime;
		
		private EntityRef<Patient> _Patient;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnStudyIdChanging(System.Guid value);
    partial void OnStudyIdChanged();
    partial void OnExternalStudyIDChanging(string value);
    partial void OnExternalStudyIDChanged();
    partial void OnDescriptionChanging(string value);
    partial void OnDescriptionChanged();
    partial void OnPerformedDateTimeChanging(System.Nullable<System.DateTime> value);
    partial void OnPerformedDateTimeChanged();
    partial void OnPatientIdChanging(System.Nullable<System.Guid> value);
    partial void OnPatientIdChanged();
    partial void OnIndexChanging(System.Nullable<int> value);
    partial void OnIndexChanged();
    partial void OnStudyInstanceUidChanging(string value);
    partial void OnStudyInstanceUidChanged();
    partial void OnRootStoragePathChanging(string value);
    partial void OnRootStoragePathChanged();
    partial void OnCreatedDateTimeChanging(System.Nullable<System.DateTime> value);
    partial void OnCreatedDateTimeChanged();
    partial void OnCommentChanging(string value);
    partial void OnCommentChanged();
    partial void OnModalityAggregationChanging(string value);
    partial void OnModalityAggregationChanged();
    partial void OnSenderIdChanging(System.Nullable<System.Guid> value);
    partial void OnSenderIdChanged();
    partial void OnAccessionNumberChanging(string value);
    partial void OnAccessionNumberChanged();
    partial void OnOtherPatientIdChanging(string value);
    partial void OnOtherPatientIdChanged();
    partial void OnNameOfPhysiciansReadingStudyChanging(string value);
    partial void OnNameOfPhysiciansReadingStudyChanged();
    partial void OnVeterinarianChanging(string value);
    partial void OnVeterinarianChanged();
    partial void OnTechnicianChanging(string value);
    partial void OnTechnicianChanged();
    partial void OnReferringPhysiciansNameChanging(string value);
    partial void OnReferringPhysiciansNameChanged();
    partial void OnAssignedDocumentCollectionIdChanging(System.Nullable<System.Guid> value);
    partial void OnAssignedDocumentCollectionIdChanged();
    partial void OnPropertiesChanging(System.Xml.Linq.XElement value);
    partial void OnPropertiesChanged();
    partial void OnLastChangedDateTimeChanging(System.Nullable<System.DateTime> value);
    partial void OnLastChangedDateTimeChanged();
    #endregion
		
		public Study()
		{
			this._Patient = default(EntityRef<Patient>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StudyId", DbType="UniqueIdentifier NOT NULL", IsPrimaryKey=true)]
		public System.Guid StudyId
		{
			get
			{
				return this._StudyId;
			}
			set
			{
				if ((this._StudyId != value))
				{
					this.OnStudyIdChanging(value);
					this.SendPropertyChanging();
					this._StudyId = value;
					this.SendPropertyChanged("StudyId");
					this.OnStudyIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ExternalStudyID", DbType="NVarChar(20)")]
		public string ExternalStudyID
		{
			get
			{
				return this._ExternalStudyID;
			}
			set
			{
				if ((this._ExternalStudyID != value))
				{
					this.OnExternalStudyIDChanging(value);
					this.SendPropertyChanging();
					this._ExternalStudyID = value;
					this.SendPropertyChanged("ExternalStudyID");
					this.OnExternalStudyIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Description", DbType="NVarChar(1000)")]
		public string Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				if ((this._Description != value))
				{
					this.OnDescriptionChanging(value);
					this.SendPropertyChanging();
					this._Description = value;
					this.SendPropertyChanged("Description");
					this.OnDescriptionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PerformedDateTime", DbType="DateTime")]
		public System.Nullable<System.DateTime> PerformedDateTime
		{
			get
			{
				return this._PerformedDateTime;
			}
			set
			{
				if ((this._PerformedDateTime != value))
				{
					this.OnPerformedDateTimeChanging(value);
					this.SendPropertyChanging();
					this._PerformedDateTime = value;
					this.SendPropertyChanged("PerformedDateTime");
					this.OnPerformedDateTimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PatientId", DbType="UniqueIdentifier")]
		public System.Nullable<System.Guid> PatientId
		{
			get
			{
				return this._PatientId;
			}
			set
			{
				if ((this._PatientId != value))
				{
					if (this._Patient.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnPatientIdChanging(value);
					this.SendPropertyChanging();
					this._PatientId = value;
					this.SendPropertyChanged("PatientId");
					this.OnPatientIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Name="[Index]", Storage="_Index", DbType="Int")]
		public System.Nullable<int> Index
		{
			get
			{
				return this._Index;
			}
			set
			{
				if ((this._Index != value))
				{
					this.OnIndexChanging(value);
					this.SendPropertyChanging();
					this._Index = value;
					this.SendPropertyChanged("Index");
					this.OnIndexChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StudyInstanceUid", DbType="NVarChar(100)")]
		public string StudyInstanceUid
		{
			get
			{
				return this._StudyInstanceUid;
			}
			set
			{
				if ((this._StudyInstanceUid != value))
				{
					this.OnStudyInstanceUidChanging(value);
					this.SendPropertyChanging();
					this._StudyInstanceUid = value;
					this.SendPropertyChanged("StudyInstanceUid");
					this.OnStudyInstanceUidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_RootStoragePath", DbType="NVarChar(512)")]
		public string RootStoragePath
		{
			get
			{
				return this._RootStoragePath;
			}
			set
			{
				if ((this._RootStoragePath != value))
				{
					this.OnRootStoragePathChanging(value);
					this.SendPropertyChanging();
					this._RootStoragePath = value;
					this.SendPropertyChanged("RootStoragePath");
					this.OnRootStoragePathChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CreatedDateTime", DbType="DateTime")]
		public System.Nullable<System.DateTime> CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				if ((this._CreatedDateTime != value))
				{
					this.OnCreatedDateTimeChanging(value);
					this.SendPropertyChanging();
					this._CreatedDateTime = value;
					this.SendPropertyChanged("CreatedDateTime");
					this.OnCreatedDateTimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Comment", DbType="NText", UpdateCheck=UpdateCheck.Never)]
		public string Comment
		{
			get
			{
				return this._Comment;
			}
			set
			{
				if ((this._Comment != value))
				{
					this.OnCommentChanging(value);
					this.SendPropertyChanging();
					this._Comment = value;
					this.SendPropertyChanged("Comment");
					this.OnCommentChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ModalityAggregation", DbType="NVarChar(50)")]
		public string ModalityAggregation
		{
			get
			{
				return this._ModalityAggregation;
			}
			set
			{
				if ((this._ModalityAggregation != value))
				{
					this.OnModalityAggregationChanging(value);
					this.SendPropertyChanging();
					this._ModalityAggregation = value;
					this.SendPropertyChanged("ModalityAggregation");
					this.OnModalityAggregationChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SenderId", DbType="UniqueIdentifier")]
		public System.Nullable<System.Guid> SenderId
		{
			get
			{
				return this._SenderId;
			}
			set
			{
				if ((this._SenderId != value))
				{
					this.OnSenderIdChanging(value);
					this.SendPropertyChanging();
					this._SenderId = value;
					this.SendPropertyChanged("SenderId");
					this.OnSenderIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_AccessionNumber", DbType="NVarChar(50)")]
		public string AccessionNumber
		{
			get
			{
				return this._AccessionNumber;
			}
			set
			{
				if ((this._AccessionNumber != value))
				{
					this.OnAccessionNumberChanging(value);
					this.SendPropertyChanging();
					this._AccessionNumber = value;
					this.SendPropertyChanged("AccessionNumber");
					this.OnAccessionNumberChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_OtherPatientId", DbType="NVarChar(50)")]
		public string OtherPatientId
		{
			get
			{
				return this._OtherPatientId;
			}
			set
			{
				if ((this._OtherPatientId != value))
				{
					this.OnOtherPatientIdChanging(value);
					this.SendPropertyChanging();
					this._OtherPatientId = value;
					this.SendPropertyChanged("OtherPatientId");
					this.OnOtherPatientIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NameOfPhysiciansReadingStudy", DbType="NVarChar(50)")]
		public string NameOfPhysiciansReadingStudy
		{
			get
			{
				return this._NameOfPhysiciansReadingStudy;
			}
			set
			{
				if ((this._NameOfPhysiciansReadingStudy != value))
				{
					this.OnNameOfPhysiciansReadingStudyChanging(value);
					this.SendPropertyChanging();
					this._NameOfPhysiciansReadingStudy = value;
					this.SendPropertyChanged("NameOfPhysiciansReadingStudy");
					this.OnNameOfPhysiciansReadingStudyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Veterinarian", DbType="NVarChar(50)")]
		public string Veterinarian
		{
			get
			{
				return this._Veterinarian;
			}
			set
			{
				if ((this._Veterinarian != value))
				{
					this.OnVeterinarianChanging(value);
					this.SendPropertyChanging();
					this._Veterinarian = value;
					this.SendPropertyChanged("Veterinarian");
					this.OnVeterinarianChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Technician", DbType="NVarChar(50)")]
		public string Technician
		{
			get
			{
				return this._Technician;
			}
			set
			{
				if ((this._Technician != value))
				{
					this.OnTechnicianChanging(value);
					this.SendPropertyChanging();
					this._Technician = value;
					this.SendPropertyChanged("Technician");
					this.OnTechnicianChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ReferringPhysiciansName", DbType="NVarChar(200)")]
		public string ReferringPhysiciansName
		{
			get
			{
				return this._ReferringPhysiciansName;
			}
			set
			{
				if ((this._ReferringPhysiciansName != value))
				{
					this.OnReferringPhysiciansNameChanging(value);
					this.SendPropertyChanging();
					this._ReferringPhysiciansName = value;
					this.SendPropertyChanged("ReferringPhysiciansName");
					this.OnReferringPhysiciansNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_AssignedDocumentCollectionId", DbType="UniqueIdentifier")]
		public System.Nullable<System.Guid> AssignedDocumentCollectionId
		{
			get
			{
				return this._AssignedDocumentCollectionId;
			}
			set
			{
				if ((this._AssignedDocumentCollectionId != value))
				{
					this.OnAssignedDocumentCollectionIdChanging(value);
					this.SendPropertyChanging();
					this._AssignedDocumentCollectionId = value;
					this.SendPropertyChanged("AssignedDocumentCollectionId");
					this.OnAssignedDocumentCollectionIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Properties", DbType="Xml", UpdateCheck=UpdateCheck.Never)]
		public System.Xml.Linq.XElement Properties
		{
			get
			{
				return this._Properties;
			}
			set
			{
				if ((this._Properties != value))
				{
					this.OnPropertiesChanging(value);
					this.SendPropertyChanging();
					this._Properties = value;
					this.SendPropertyChanged("Properties");
					this.OnPropertiesChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastChangedDateTime", DbType="DateTime")]
		public System.Nullable<System.DateTime> LastChangedDateTime
		{
			get
			{
				return this._LastChangedDateTime;
			}
			set
			{
				if ((this._LastChangedDateTime != value))
				{
					this.OnLastChangedDateTimeChanging(value);
					this.SendPropertyChanging();
					this._LastChangedDateTime = value;
					this.SendPropertyChanged("LastChangedDateTime");
					this.OnLastChangedDateTimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Patient_Study", Storage="_Patient", ThisKey="PatientId", OtherKey="PatientId", IsForeignKey=true)]
		public Patient Patient
		{
			get
			{
				return this._Patient.Entity;
			}
			set
			{
				Patient previousValue = this._Patient.Entity;
				if (((previousValue != value) 
							|| (this._Patient.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Patient.Entity = null;
						previousValue.Studies.Remove(this);
					}
					this._Patient.Entity = value;
					if ((value != null))
					{
						value.Studies.Add(this);
						this._PatientId = value.PatientId;
					}
					else
					{
						this._PatientId = default(Nullable<System.Guid>);
					}
					this.SendPropertyChanged("Patient");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591
