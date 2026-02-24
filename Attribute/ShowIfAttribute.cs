using System;
using UnityEngine;

namespace Cyan {
    public class ShowIfAttribute : PropertyAttribute {

        public string property;
        public object value;

        public ShowIfAttribute(string property) {
            this.property = property;
        }

        public ShowIfAttribute(string property, object value) {
            this.property = property;
            this.value = value;
        }

    }
}