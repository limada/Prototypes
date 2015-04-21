/*
 * Limada
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Id = System.Int64;

namespace Limada.Model {

    public class IdLink : ILink<Id> {

        public Id Id { get; set; }

        public Id Marker { get; set; }

        public Id Root { get; set; }

        public Id Leaf { get; set; }
    }
}
