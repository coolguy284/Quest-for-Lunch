using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using TMPro;

public class EnItem : MonoBehaviour {
    public GameObject UISlots;
    EnMain EnMainInst;
    EnHealth EnHealthInst;

    public class Slot {
        public string name;
        public object[] extra;
        public Slot() {
            name = "";
            extra = new object[0] {};
        }
        public Slot(string nameVal) {
            name = nameVal;
            if (name == "Weapon Plus") {
                extra = new object[1] {0};
            } else {
                extra = new object[0] {};
            }
        }
    }

    public Slot[] Slots = new Slot[2] { new Slot(), new Slot() };

    void DisplaySlot(Slot slot, Transform slotUI) {
        slotUI.Find("Item").GetComponent<Image>().sprite = slot.name == "" ? null : EnMainInst.SpriteDict[slot.name];
        slotUI.Find("Item").GetComponent<Image>().color = slot.name == "" ? Color.clear : Color.white;
        if (slot.name == "Weapon Plus") {
            slotUI.Find("Info").gameObject.SetActive(true);
            slotUI.Find("Info").GetComponent<TextMeshProUGUI>().text = (EnMainInst.WeaponStats[slot.name].extraInt[0] - (int)slot.extra[0]).ToString() + "/" + EnMainInst.WeaponStats[slot.name].extraInt[0].ToString();
        } else {
            slotUI.Find("Info").gameObject.SetActive(false);
        }
    }

    public void DisplaySingleSlot(int slot) {
        DisplaySlot(Slots[slot], UISlots.transform.GetChild(slot));
    }

    public void DisplaySlots(Transform slots = null) {
        if (slots == null) slots = UISlots.transform;
        DisplaySlot(Slots[0], slots.GetChild(0));
        DisplaySlot(Slots[1], slots.GetChild(1));
    }

    int GetFreeSlot() {
        if (Slots[0].name == "") {
            return 0;
        } else if (Slots[1].name == "") {
            return 1;
        } else {
            return -1;
        }
    }

    int GetTakenSlot() {
        if (Slots[0].name != "") {
            return 0;
        } else if (Slots[1].name != "") {
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
                var slot = entity.GetComponent<SlotStore>().Slot;
                if (slot != null) {
                    Slots[freeSlot] = slot;
                } else {
                    Slots[freeSlot] = new Slot(entity.GetComponent<SpriteRenderer>().sprite.name);
                }
                Destroy(entity);
                DisplaySlots();
            }
        }
    }

    public void DropItem(int slot) {
        if (slot < 0 || slot > Slots.Length || Slots[slot].name == "" || !EnHealthInst.alive) return;
        var item = Instantiate(EnMainInst.ItemPrefab, Vector3.zero, Quaternion.identity);
        item.GetComponent<SpriteRenderer>().sprite = EnMainInst.SpriteDict[Slots[slot].name];
        item.transform.parent = EnMainInst.ItemsList.transform;
        item.transform.localPosition = transform.position;
        item.name = "Item " + Slots[slot].name;
        item.GetComponent<SlotStore>().Slot = Slots[slot];
        Slots[slot] = new Slot("");
        DisplaySlots();
    }

    void SwapItems(int slot1, int slot2) {
        var temp = Slots[slot1];
        Slots[slot1] = Slots[slot2];
        Slots[slot2] = temp;
        DisplaySlots();
    }

    public void SwapItems() {
        SwapItems(0, 1);
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
