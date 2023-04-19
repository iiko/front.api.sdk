using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Exceptions;
using Resto.Front.Api.Attributes.JetBrains;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    public sealed partial class ClientBox
    {
        [CanBeNull]
        private readonly IClient existingClient;
        [CanBeNull]
        public IClient CreatedClient { get; private set; }

        public ClientBox()
        {

        }

        public ClientBox([CanBeNull] IClient existingClient)
        {
            this.existingClient = existingClient;
            InitializeComponent();
            DataContext = existingClient;
            if (existingClient == null)
            {
                btnOK.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Hidden;
            }
            else
            {
                btnOK.Visibility = Visibility.Hidden;
                btnSave.Visibility = Visibility.Visible;
            }
        }

        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            var editSession = PluginContext.Operations.CreateEditSession();
            var clientStub = editSession.CreateClient(Guid.NewGuid(), txtName.Text, ConvertTextToPhonesList(txtPhone.Text), null, DateTime.Now);
            editSession.ChangeClientSurname(txtSurname.Text, clientStub);
            editSession.ChangeClientNick(txtNick.Text, clientStub);
            editSession.ChangeClientCardNumber(txtCard.Text, clientStub);
            // editSession.ChangeClientAddresses(ConvertTextToAddressesList(txtAddress.Text), 0, clientStub);
            editSession.ChangeClientEmails(ConvertTextToEmailsList(txtEmail.Text), clientStub);
            editSession.ChangeClientComment(txtComment.Text, clientStub);

            var createdEntities = PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);
            CreatedClient = createdEntities.Get(clientStub);
            DialogResult = true;
        }

        [NotNull]
        private static List<EmailDto> ConvertTextToEmailsList([NotNull] string emailsText)
        {
            return emailsText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select((emailText, emailIndex) =>
                    new EmailDto
                    {
                        EmailValue = emailText,
                        IsMain = emailIndex == 0
                    })
                .ToList();
        }

        [NotNull]
        private static List<PhoneDto> ConvertTextToPhonesList([NotNull] string phonesText)
        {
            return phonesText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select((phoneText, phoneIndex) =>
                    new PhoneDto
                    {
                        PhoneValue = phoneText,
                        IsMain = phoneIndex == 0
                    })
                .ToList();
        }

        private void BtnSaveClick(object sender, RoutedEventArgs e)
        {
            Debug.Assert(existingClient != null);
            var editSession = PluginContext.Operations.CreateEditSession();

            editSession.ChangeClientName(txtName.Text, existingClient);
            editSession.ChangeClientSurname(txtSurname.Text, existingClient);
            editSession.ChangeClientNick(txtNick.Text, existingClient);
            editSession.ChangeClientCardNumber(txtCard.Text, existingClient);
            editSession.ChangeClientPhones(ConvertTextToPhonesList(txtPhone.Text), existingClient);
            // editSession.ChangeClientAddresses(ConvertTextToAddressesList(txtAddress.Text), 0, existingClient);
            editSession.ChangeClientEmails(ConvertTextToEmailsList(txtEmail.Text), existingClient);
            editSession.ChangeClientComment(txtComment.Text, existingClient);

            try
            {
                PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);
            }
            catch (EntityAlreadyInUseException exception)
            {
                MessageBox.Show("Entity is already in use. Try again later." + Environment.NewLine + exception);
            }
            catch (EntityModifiedException exception)
            {
                MessageBox.Show("Refresh list and try again." + Environment.NewLine + exception);
            }
            DialogResult = true;
        }
    }
}
