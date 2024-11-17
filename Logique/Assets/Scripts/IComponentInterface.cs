using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IComponentInterface
{
    bool inputA { get; set; }
    bool outputA { get; }
    bool inputB { get; set; }
    bool outputB { get; }
    event Action SignalUpdated;
    bool IsConnected(Collider2D collider);
    void SetConnection(Collider2D collider, bool isConnected);
}
