using System;
using System.Collections.Generic;
using System.Text;

namespace perimapp.Models
{
    public enum SyncState
    {
        Synced = 0,

        // 1: Produit scanné hors-ligne.
        PendingCreate = 1,

        // 2: Produit existant modifié hors-ligne.
        PendingUpdate = 2,

        // 3: Produit supprimé hors-ligne
        PendingDelete = 3
    }
}