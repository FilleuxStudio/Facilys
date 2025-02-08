namespace Facilys.Components.Models.Modal
{
    public class ModalManagerId
    {
        private readonly Dictionary<string, Modal> modals = [];
        public bool IsBackdropVisible { get; private set; } = false;

        public void RegisterModal(string id)
        {
            if (!modals.ContainsKey(id))
            {
                modals[id] = new Modal();
            }
        }

        public Modal GetModal(string id)
        {
            return modals.TryGetValue(id, out var modal) ? modal : null;
        }

        public void OpenModal(string id)
        {
            if (modals.ContainsKey(id))
            {
                modals[id].OpenModal(id);
                IsBackdropVisible = true; // Activer le fond grisé
            }
        }

        public void CloseModal(string id)
        {
            if (modals.ContainsKey(id))
            {
                modals[id].CloseModal(id);
                IsBackdropVisible = modals.Any(m => m.Value.IsOpen);
            }
        }
    }
    public class Modal
    {
        public string ModalDisplay { get; set; } = "display: none;";
        public string ModalClass { get; set; } = "modal fade bd-example-modal-lg";
        public string AriaHidden { get; set; } = "true";
        public string ShowClass { get; set; } = "";
        public string targetId { get; set; } = "";
        public bool IsOpen { get; private set; } = false;


        public void OpenModal(string idModal)
        {
            ModalDisplay = "display: block;";
            ModalClass = "modal fade bd-example-modal-lg show";
            AriaHidden = "false";
            ShowClass = "show";
            targetId = idModal;
            IsOpen = true;
        }

        public void CloseModal(string idModal)
        {
            // Modifier les attributs pour fermer le modal
            ModalDisplay = "display: none;";
            ModalClass = "modal fade bd-example-modal-lg";
            AriaHidden = "true";
            ShowClass = "";
            targetId = idModal;
            IsOpen = false;
        }
    }
}
