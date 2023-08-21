using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace InWebo.ApiDemo
{

    /// <summary>
    ///   This class describes a login.
    /// </summary>
    public class User
    {
        public static readonly Hashtable HashedProperties;

        // static class constructor to initialize constant
        static User()
        {
            HashedProperties = new Hashtable();
            foreach (PropertyInfo p in typeof(User).GetProperties())
            {
                HashedProperties.Add(p.Name.ToLower(), p);
            }
        }

        // Properties
        public long? Id { get; set; }
        public string Code { get; set; }
        public long? Status { get; set; }
        public long? Role { get; set; }
        public string Firstname { get; set; }
        public string Name { get; set; }
        public string Mail { get; set; }
        public string Login { get; set; }
        public string Phone { get; set; }
        public long? CreatedBy { get; set; }
        public long? Activation_Status { get; set; }
        public string ExtraFields { get; set; }

        public User()
        {

        }

        public User(long id)
            : this()
        {
            this.Id = id;
        }

        public void DisplayInfo()
        {
            const string undefined = "<undefined>";
            string stringValue;
            foreach (PropertyInfo prop in GetType().GetProperties())
            {

                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    long? value = (long?) prop.GetValue(this, null) ;
                    stringValue =value.HasValue ? value.Value.ToString() : undefined;
                } else {
                    stringValue = (string) prop.GetValue(this, null);
                    if (stringValue == null) stringValue = undefined;
                }
                Console.Write("\n{0,-18}: {1}", prop.Name, stringValue);
            }

        }

        

        public static User FromLoginResult(object result)
        {
            if (result == null) return null;
            User user = new User();
            // We get the properties of the LoginQueryResult object and of User.
            PropertyInfo[] queryResultProperties = result.GetType().GetProperties();

            // For each User property, we check that it exists in LoginQueryResult
            // and we copy the value in LoginsQueryResult into the User property.
            foreach (PropertyInfo resultProperty in queryResultProperties)
            {
                PropertyInfo userProperty = (PropertyInfo)User.HashedProperties[resultProperty.Name];
                if (userProperty != null)
                {
                    object resultValue = resultProperty.GetValue(result, null);
                    user.CopyResultProperty(userProperty, resultProperty, resultValue);
                }
            }
            return user;
        }

        public void CopyResultProperty(PropertyInfo userProp, PropertyInfo resultProp, object resultValue)
        {
            if (resultProp.PropertyType.IsGenericType && resultProp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            //if (resultProp.PropertyType.IsGenericType && resultProp.PropertyType == typeof(Nullable<>))
            {
                CopyNullableProperty(userProp, resultValue);
            }
            else
            {
                userProp.SetValue(this, resultValue, null);
            }
        }

        public void CopyNullableProperty(PropertyInfo userProperty, object resultValue)
        {
            if (((long?)resultValue).HasValue)
            {
                userProperty.SetValue(this, ((long?)resultValue).Value, null);
            }
        }
        public void CopyNullableProperty(PropertyInfo userProperty, object resultValue, int index)
        {
            if (((long?[])resultValue)[index].HasValue)
            {
                userProperty.SetValue(this, ((long?[])resultValue)[index].Value, null);
            }
        }

        public void CopyStandardProperty(PropertyInfo userProperty, object resultValue)
        {
            userProperty.SetValue(this, resultValue, null);
        }

        public void CopyStandardProperty(PropertyInfo userProperty, object resultValue, int index)
        {
            userProperty.SetValue(this, ((object[])resultValue)[index], null);
        }

    } // class User

    public class UserList : List<User>
    {
        public UserList() : base() {}

        /// <summary>
        /// Creates a list of logins from a LoginsQueryResult
        /// In LoginsQueryResult, each property is an array, containing the values for each user.
        /// </summary>
        /// <param name="result">Result from loginsQuery</param>
        /// <seealso cref="InWebo.Api.Utils.Provisionning.loginsQuery"/>
        /// <returns>List of user</returns>
        public static UserList FromLoginsResult(object result)
        {
            if (result == null) return null;
            UserList users = new UserList();

            // We get the properties of the LoginsQueryResult object and of User.
            PropertyInfo[] queryResultProperties = result.GetType().GetProperties();

            // For each LoginsQueryResult property, we check that it exists in User
            // and we copy the value in LoginsQueryResult into the User property.
            foreach (PropertyInfo resultProperty in queryResultProperties)
            {
                PropertyInfo userProperty = (PropertyInfo)User.HashedProperties[resultProperty.Name];
                if (userProperty == null) continue;

                // We found a matching property

                object resultValue = resultProperty.GetValue(result, null);

                Type underlyingType = resultProperty.PropertyType.GetElementType();
                if (underlyingType == null)
                { 
                    // this is a single user.
                    User user = User.FromLoginResult(result);
                    if (user != null) users.Add(user);
                    return users;
                }
                
                // Check if the underlying type is Nullable
                bool nullable = false;
                int nbItems = 0;
                if (underlyingType.IsGenericType && (underlyingType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    nullable = true;
                    nbItems = ((long?[])resultValue).Length - 1; // the last element is null
                }
                else
                {
                    nullable = false;
                    nbItems = ((object[])resultValue).Length - 1; // the last element is null
                }

                // let's copy the values into corresponding new User objects' properties.
                for (int i = 0; i < nbItems; i++)
                {
                    if (users.Count == i)
                    {
                        users.Add(new User());
                    }

                    // For Nullable properties, we must check HasValue before accessing the value;
                    if (nullable)
                    {
                        users[i].CopyNullableProperty(userProperty, resultValue, i);
                    }
                    else
                    {
                        users[i].CopyStandardProperty(userProperty, resultValue, i);
                    }
                }
                

            }
            return users;
        }

        public void DisplayInfo()
        {
            int nb = Count;
            for (int i = 0; i < nb; i++)
            {
                Console.WriteLine("\n\nUser #{0}/{1}", i+1,nb);
                Console.Write("-----------------------------------");
                this[i].DisplayInfo();
            }

        }

    }
} // namespace
