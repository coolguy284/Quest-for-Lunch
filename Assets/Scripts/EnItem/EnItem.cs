using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class EnItem : MonoBehaviour {
    public GameObject UISlots;
    EnMain EnMainInst;
    EnHealth EnHealthInst;
    public string[] Slots = new string[2] { "", "" };

    void DisplaySlot(Image image, string name) {
        image.sprite = name == "" ? null : EnMainInst.SpriteDict[name];
        image.color = name == "" ? Color.clear : Color.white;
    }

    public void DisplaySlots(Transform slots = null) {
        if (slots == null) slots = UISlots.transform;
        DisplaySlot(slots.GetChild(0).Find("Item").GetComponent<Image>(), Slots[0]);
        DisplaySlot(slots.GetChild(1).Find("Item").GetComponent<Image>(), Slots[1]);
    }

    int GetFreeSlot() {
        if (Slots[0] == "") {
            return 0;
        } else if (Slots[1] == "") {
            return 1;
        } else {
            return -1;
        }
    }

    int GetTakenSlot() {
        if (Slots[0] != "") {
            return 0;
        } else if (Slots[1] != "") {
            return 1;
        } else {
            return -1;
        }
    }

    void PickupItem() {
        if (!EnHealthInst.alive) return;
        var freeSlot = GetFreeSlot();
        if (freeSlot > -1) {
            var collidersList = transform.Find("ItemGrabBox").GetComponent<ColliderTracker>().colliders;
            if (collidersList.Count != 0) {
                var entity = collidersList[0].gameObject;
                Slots[freeSlot] = entity.GetComponent<SpriteRenderer>().sprite.name;
                Destroy(entity);
                DisplaySlots();
            }
        }
    }

    public void DropItem(int slot) {
        if (slot < 0 || slot > Slots.Length || Slots[slot] == "" || !EnHealthInst.alive) return;
        var item = Instantiate(EnMainInst.ItemPrefab, transform.position, Quaternion.identity);
        item.GetComponent<SpriteRenderer>().sprite = EnMainInst.SpriteDict[Slots[slot]];
        item.transform.parent = EnMainInst.ItemsList.transform;
        item.name = "Item " + Slots[slot];
        Slots[slot] = "";
        DisplaySlots();
    }

    void Start() {
        EnMainInst = GetComponent<EnMain>();
        EnHealthInst = GetComponent<EnHealth>();
    }

    void Update() {
        if (EnMainInst == null) EnMainInst = GetComponent<EnMain>();
        if (EnHealthInst.alive && EnMainInst.inputs.pickupItem) {
            PickupItem();
        }
        if (EnHealthInst.alive && EnMainInst.inputs.dropItem) {
            DropItem(GetTakenSlot());
        }
    }
}
