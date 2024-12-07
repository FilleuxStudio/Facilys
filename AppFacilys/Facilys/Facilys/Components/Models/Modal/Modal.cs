namespace Facilys.Components.Models.Modal
{
    public class Modal
    {
        public string ModalDisplay { get; set; } = "display: none;";
        public string ModalClass { get; set; } = "modal fade bd-example-modal-lg";
        public string AriaHidden { get; set; } = "true";
        public string ShowClass { get; set; } = "";
        public string targetId { get; set; } = "";


        public void OpenModal(string idModal)
        {
            ModalDisplay = "display: block;";
            ModalClass = "modal fade bd-example-modal-lg show";
            AriaHidden = "false";
            ShowClass = "show";
            targetId = idModal;
        }

        public void CloseModal(string idModal)
        {
            // Modifier les attributs pour fermer le modal
            ModalDisplay = "display: none;";
            ModalClass = "modal fade bd-example-modal-lg";
            AriaHidden = "true";
            ShowClass = "";
            targetId = idModal;
        }
    }
}
