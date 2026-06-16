using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class AutoBindCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class BindComponentInfo
	{
		public enum EFieldXuiType
		{
			Controller,
			View
		}

		public enum EMultiplicity
		{
			Single,
			Array
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly FieldInfo targetField;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EFieldXuiType fieldXuiType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EMultiplicity multiplicity;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Type lookupType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string lookupId;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool required;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool parentLookup;

		public BindComponentInfo(FieldInfo _targetField, EFieldXuiType _fieldXuiType, EMultiplicity _multiplicity, Type _lookupType, string _lookupId, bool _required, bool _isParentLookup)
		{
			targetField = _targetField;
			fieldXuiType = _fieldXuiType;
			multiplicity = _multiplicity;
			lookupType = _lookupType;
			lookupId = _lookupId ?? "";
			required = _required;
			parentLookup = _isParentLookup;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void bindParent(XUiController _controller)
		{
			bool flag;
			object value;
			if (fieldXuiType == EFieldXuiType.Controller)
			{
				flag = _controller.TryGetParentController(lookupType, out var _parent);
				value = _parent;
			}
			else
			{
				flag = _controller.TryGetParentView(lookupType, out var _parent2);
				value = _parent2;
			}
			if (flag)
			{
				targetField.SetValue(_controller, value);
			}
			else if (required)
			{
				LogNoViewFound(_controller, "Could not find matching controller / view for auto bind parent field.");
			}
		}

		public void Bind(XUiController _controller)
		{
			if (parentLookup)
			{
				bindParent(_controller);
				return;
			}
			XUiController xUiController = _controller;
			ReadOnlyMemory<char> id = lookupId.AsMemory();
			for (int num = id.Span.IndexOf('.'); num > 0; num = id.Span.IndexOf('.'))
			{
				ReadOnlyMemory<char> id2 = id.Slice(0, num);
				id = id.Slice(num + 1);
				if (xUiController.TryGetChildController(typeof(XUiController), id2, out var _child))
				{
					xUiController = _child;
				}
				else if (required)
				{
					LogNoViewFound(_controller, "Could not find matching parent view for auto bind field.");
					return;
				}
			}
			switch (multiplicity)
			{
			case EMultiplicity.Single:
			{
				bool flag;
				object value;
				if (fieldXuiType == EFieldXuiType.Controller)
				{
					flag = xUiController.TryGetChildController(lookupType, id, out var _child2);
					value = _child2;
				}
				else
				{
					flag = xUiController.TryGetChildView(lookupType, id, out var _child3);
					value = _child3;
				}
				if (flag)
				{
					targetField.SetValue(_controller, value);
				}
				else if (required)
				{
					LogNoViewFound(_controller, "Could not find matching view for auto bind field.");
				}
				break;
			}
			case EMultiplicity.Array:
			{
				IList list;
				if (fieldXuiType == EFieldXuiType.Controller)
				{
					list = controllersList;
					list.Clear();
					xUiController.GetChildControllers(lookupType, id, controllersList);
				}
				else
				{
					list = viewsList;
					list.Clear();
					xUiController.GetChildViews(lookupType, id, viewsList);
				}
				Array array = Array.CreateInstance(targetField.FieldType.GetElementType(), list.Count);
				for (int i = 0; i < list.Count; i++)
				{
					array.SetValue(list[i], i);
				}
				targetField.SetValue(_controller, array);
				if (list.Count == 0 && required)
				{
					LogNoViewFound(_controller, "Could not find any matching views for auto bind field.");
				}
				list.Clear();
				break;
			}
			default:
				Log.Error($"[XUi] Auto bind field: Multiplicity value '{multiplicity}' not known.");
				break;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LogNoViewFound(XUiController _controller, string _message)
		{
			Log.Error(string.IsNullOrEmpty(lookupId) ? ("[XUi] " + _message + " " + targetField.DeclaringType.FullName + "." + targetField.Name + " (expected type: " + lookupType.Name + "). Hierarchy: " + _controller.GetXuiHierarchy()) : ("[XUi] " + _message + " " + targetField.DeclaringType.FullName + "." + targetField.Name + " (expected type: " + lookupType.Name + ", expected name: '" + lookupId + "'). Hierarchy: " + _controller.GetXuiHierarchy()));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class BindEventInfo
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EventInfo eventInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly FieldInfo eventComponentField;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly MethodInfo eventMethod;

		public BindEventInfo(EventInfo _eventInfo, FieldInfo _eventComponentField, MethodInfo _eventMethod)
		{
			eventInfo = _eventInfo;
			eventComponentField = _eventComponentField;
			eventMethod = _eventMethod;
		}

		public void Bind(XUiController _controller)
		{
			Delegate handler = eventMethod.CreateDelegate(eventInfo.EventHandlerType, _controller);
			if (eventComponentField == null)
			{
				eventInfo.AddEventHandler(_controller, handler);
				return;
			}
			if (!eventComponentField.FieldType.IsArray)
			{
				object value = eventComponentField.GetValue(_controller);
				if (value == null)
				{
					if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
					{
						Log.Warning("[XUi] Failed binding event on component to method: Component field is null. Field " + eventComponentField.DeclaringType.FullName + "." + eventComponentField.Name);
					}
				}
				else
				{
					eventInfo.AddEventHandler(value, handler);
				}
				return;
			}
			Array array = (Array)eventComponentField.GetValue(_controller);
			if (array == null)
			{
				if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
				{
					Log.Warning("[XUi] Failed binding event on components array to method: Component array field is null. Field " + eventComponentField.DeclaringType.FullName + "." + eventComponentField.Name);
				}
				return;
			}
			for (int i = 0; i < array.Length; i++)
			{
				object value2 = array.GetValue(i);
				if (value2 == null)
				{
					Log.Error($"[XUi] Failed binding event on component list to method: Entry {i} is null. Field {eventComponentField.DeclaringType.FullName}.{eventComponentField.Name}");
				}
				else
				{
					eventInfo.AddEventHandler(value2, handler);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class AutoBindTypeCache
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Type controllerType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<BindComponentInfo> fields = new List<BindComponentInfo>(64);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<BindEventInfo> events = new List<BindEventInfo>();

		public AutoBindTypeCache(Type _controllerType)
		{
			controllerType = _controllerType;
			initCacheForController();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void initCacheForController()
		{
			RecurseTypeTree(controllerType);
			[PublicizedFrom(EAccessModifier.Private)]
			void ComponentFieldFoundCallback(FieldInfo _fieldInfo, bool _hasMultiple, XuiBindComponentAttribute _attribute)
			{
				FieldFound(_fieldInfo, _hasMultiple, _attribute.XmlElementName, _attribute.Required, _isParentLookup: false);
			}
			[PublicizedFrom(EAccessModifier.Private)]
			void EventMethodFoundCallback(MethodInfo _methodInfo, bool _hasMultiple, XuiBindEventAttribute _attribute)
			{
				Type declaringType = _methodInfo.DeclaringType;
				foreach (XuiBindEventAttribute customAttribute in _methodInfo.GetCustomAttributes<XuiBindEventAttribute>())
				{
					string componentFieldName = customAttribute.ComponentFieldName;
					FieldInfo fieldInfo = null;
					Type type;
					if (string.IsNullOrEmpty(componentFieldName))
					{
						type = declaringType;
					}
					else
					{
						fieldInfo = declaringType.GetField(componentFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (fieldInfo == null)
						{
							Log.Error("[XUi] Event method component field '" + componentFieldName + "' not found on type " + declaringType.FullName + ". Method " + declaringType.FullName + "." + _methodInfo.Name);
							break;
						}
						Type fieldType = fieldInfo.FieldType;
						type = (fieldType.IsArray ? fieldType.GetElementType() : fieldType);
					}
					string targetEvent = customAttribute.TargetEvent;
					if (string.IsNullOrEmpty(targetEvent))
					{
						Log.Error("[XUi] Event method without non-empty event name. Method " + declaringType.FullName + "." + _methodInfo.Name);
						break;
					}
					EventInfo eventInfo = type.GetEvent(targetEvent, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (eventInfo == null)
					{
						Log.Error("[XUi] Event method target event '" + targetEvent + "' not found on type '" + type.FullName + "'. Method " + declaringType.FullName + "." + _methodInfo.Name);
						break;
					}
					if (!ReflectionHelpers.MethodCompatibleWithDelegate(eventInfo.EventHandlerType, _methodInfo))
					{
						Log.Error("[XUi] Event method incompatible with target event '" + type.FullName + "." + targetEvent + "'. Method " + declaringType.FullName + "." + _methodInfo.Name);
						break;
					}
					BindEventInfo item = new BindEventInfo(eventInfo, fieldInfo, _methodInfo);
					events.Add(item);
				}
			}
			[PublicizedFrom(EAccessModifier.Private)]
			void FieldFound(FieldInfo _fieldInfo, bool _hasMultiple, string _xmlElementName, bool _required, bool _isParentLookup)
			{
				Type fieldType = _fieldInfo.FieldType;
				bool isArray = fieldType.IsArray;
				if (_isParentLookup && isArray)
				{
					throw new Exception("[XUi] Field marked as XuiBindParent is an array. Field " + _fieldInfo.DeclaringType.FullName + "." + _fieldInfo.Name);
				}
				Type type = (isArray ? fieldType.GetElementType() : fieldType);
				BindComponentInfo.EFieldXuiType fieldXuiType;
				if (typeof(XUiController).IsAssignableFrom(type))
				{
					fieldXuiType = BindComponentInfo.EFieldXuiType.Controller;
				}
				else
				{
					if (!typeof(XUiView).IsAssignableFrom(type))
					{
						throw new Exception("[XUi] Field marked as XuiBindComponent is neither a XUiController nor a XUiView or descendant. Field " + _fieldInfo.DeclaringType.FullName + "." + _fieldInfo.Name);
					}
					fieldXuiType = BindComponentInfo.EFieldXuiType.View;
				}
				BindComponentInfo.EMultiplicity multiplicity = (isArray ? BindComponentInfo.EMultiplicity.Array : BindComponentInfo.EMultiplicity.Single);
				BindComponentInfo item = new BindComponentInfo(_fieldInfo, fieldXuiType, multiplicity, type, _xmlElementName, _required, _isParentLookup);
				fields.Add(item);
			}
			[PublicizedFrom(EAccessModifier.Private)]
			void ParentFieldFoundCallback(FieldInfo _fieldInfo, bool _hasMultiple, XuiBindParentAttribute _attribute)
			{
				FieldFound(_fieldInfo, _hasMultiple, null, _attribute.Required, _isParentLookup: true);
			}
			[PublicizedFrom(EAccessModifier.Private)]
			void RecurseTypeTree(Type _type)
			{
				if (!(_type == null))
				{
					RecurseTypeTree(_type.BaseType);
					ReflectionHelpers.GetFieldsWithAttribute<XuiBindComponentAttribute>(_type, ComponentFieldFoundCallback, _declaredOnly: true, _allowInstance: true, _allowStatic: false);
					ReflectionHelpers.GetFieldsWithAttribute<XuiBindParentAttribute>(_type, ParentFieldFoundCallback, _declaredOnly: true, _allowInstance: true, _allowStatic: false);
					ReflectionHelpers.GetMethodsWithAttribute<XuiBindEventAttribute>(_type, EventMethodFoundCallback, _declaredOnly: true, _allowInstance: true, _allowStatic: false);
				}
			}
		}

		public void BindComponents(XUiController _controller)
		{
			for (int i = 0; i < fields.Count; i++)
			{
				fields[i].Bind(_controller);
			}
		}

		public void BindEvents(XUiController _controller)
		{
			for (int i = 0; i < events.Count; i++)
			{
				events[i].Bind(_controller);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, AutoBindTypeCache> cachePerType = new Dictionary<Type, AutoBindTypeCache>(128);

	[PublicizedFrom(EAccessModifier.Private)]
	public static AutoBindCache instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<XUiController> controllersList = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<XUiView> viewsList = new List<XUiView>();

	public static AutoBindCache Instance => instance ?? (instance = new AutoBindCache());

	public void BindComponents(XUiController _controller)
	{
		getCache(_controller).BindComponents(_controller);
	}

	public void BindEvents(XUiController _controller)
	{
		getCache(_controller).BindEvents(_controller);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoBindTypeCache getCache(XUiController _controller)
	{
		Type type = _controller.GetType();
		if (!cachePerType.TryGetValue(type, out var value))
		{
			value = new AutoBindTypeCache(type);
			cachePerType[type] = value;
		}
		return value;
	}
}
