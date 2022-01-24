<template>
  <div>
    <slideout ref="slideout" @closeSlideout="turnOffSlideOut">
      <template v-slot:form>
        <div class="new-form" :class="{ 'slide-out-content': isFormContent }">
          <h2 class="slide-out-title">Add new client</h2>
          <v-jsoneditor v-model="newClient" :plus="false"></v-jsoneditor>
          <div class="row">
            <div class="col-sm-6">
              <div class="form-group text-right">
                <button
                  class="btn btn-w-m btn-primary mt-5 cancel"
                  type="Submit"
                  @click="turnOffSlideOut"
                >
                  Cancel
                </button>
              </div>
            </div>
            <div class="col-sm-6">
              <div class="text-right">
                <button
                  class="btn btn-w-m btn-primary mt-5 submit"
                  id="save"
                  type="submit"
                  @click="createClient"
                >
                  <i v-if="loadSpinner" class="fa fa-spinner fa-spin"></i> Save
                </button>
              </div>
            </div>
          </div>
        </div>
      </template>
      <template v-slot:details v-if="selectedItem.id">
        <div
          class="user-details"
          :class="{ 'slide-out-content': isDetailsContent }"
        >
          <h2 class="slide-out-title client-details-title">Client Details</h2>
          <div class="pretty-container">
            <vue-json-pretty
              :path="'res'"
              :data="selectedItem"
              :show-length="true"
              :highlightMouseoverNode="true"
            >
            </vue-json-pretty>
          </div>
          <div class="row">
            <div class="col-sm-6">
              <div class="form-group text-right">
                <button
                  class="btn btn-w-m btn-primary mt-5 cancel"
                  type="Submit"
                  @click="turnOffSlideOut"
                >
                  Cancel
                </button>
              </div>
            </div>
            <div class="col-sm-6">
              <div class="text-right">
                <button
                  class="btn btn-w-m btn-primary mt-5 submit"
                  type="submit"
                  @click="editItem(selectedItem)"
                >
                  Edit
                </button>
              </div>
            </div>
          </div>
        </div>
      </template>
      <template v-slot:edit>
        <div
          class="new-form"
          v-if="editSelectedItem"
          :class="{ 'slide-out-content': editSelectedItem }"
        >
          <h2 class="slide-out-title">Edit client</h2>
          <div class="json-container">
            <v-jsoneditor v-model="detailsToEdit" :plus="false"></v-jsoneditor>
          </div>
          <div class="row">
            <div class="col-sm-6">
              <div class="form-group text-right">
                <button
                  class="btn btn-w-m btn-primary mt-5 cancel"
                  type="Submit"
                  @click.prevent="returnToDetails(selectedItem)"
                >
                  Cancel
                </button>
              </div>
            </div>
            <div class="col-sm-6">
              <div class="form-group text-right">
                <button
                  class="btn btn-w-m btn-primary mt-5 submit"
                  id="submit-user-update"
                  type="Submit"
                  @click.prevent="updateClientDetails(detailsToEdit)"
                >
                  <i v-if="loadSpinner" class="fa fa-spinner fa-spin"></i> Save
                </button>
              </div>
            </div>
          </div>
        </div>
      </template>
    </slideout>
    <div class="row wrapper border-bottom white-bg page-heading">
      <breadcrumb :title="breadcrumb.title" :items="breadcrumb.items" />
      <EnvBanner />
      <add-button :button_label="button_label" @slideOut="handleSlideOut" />
    </div>
    <div class="wrapper wrapper-content">
      <Table
        ref="table"
        :theadings="clients_table.headings"
        :displayDetails="displayClientDetails"
        :rows="clients"
        :allRows="clients"
        :fields="clients_table.fields"
        @edit="setItemToEdit(clients)"
      >
      </Table>
    </div>
  </div>
</template>

<script>
import Breadcrumb from "../components/shared/Breadcrumb.vue";
import AddButton from "../components/shared/AddButton";
import Table from "../components/shared/Table";
import Slideout from "../components/shared/Slideout";
import sharedMethodsMixin from "@/mixins/sharedMethodsMixin";
import VueJsonPretty from "vue-json-pretty";
import {
  endpoints,
  client_labels,
  request,
  api_client_model,
} from "../utils/constants";
import EnvBanner from "@/components/EnvBanner.vue";

export default {
  name: "Clients",
  mixins: [sharedMethodsMixin],
  components: {
    Slideout,
    Breadcrumb,
    AddButton,
    Table,
    VueJsonPretty,
    EnvBanner,
  },
  data() {
    return {
      oldClients: [],
      clients: null,
      newClient: api_client_model,
      breadcrumb: {
        title: "Clients",
        items: [
          {
            position: 1,
            path: "/",
            label: "Home",
            isActive: false,
          },
          {
            position: 2,
            path: this.$route.path,
            label: "Clients",
            isActive: true,
          },
        ],
      },
      button_label: client_labels.client,
      clients_table: {
        headings: [
          client_labels.id,
          client_labels.client_id,
          client_labels.grant_type,
          client_labels.claims,
          client_labels.scopes,
          client_labels.redirect_uri,
          client_labels.post_logout_redirect_uri,
        ],
        fields: [
          "$.id",
          "$.clientId",
          "$.allowedGrantTypes..grantType",
          "$.claims..value",
          "$.allowedScopes..scope",
          "$.redirectUris..redirectUri",
          "$.postLogoutRedirectUris..postLogoutRedirectUri",
        ],
      },
    };
  },
  created() {
    this.fetchClients();
  },
  methods: {
    updateClientDetails(details) {
      this.updateItemDetails(
        `${endpoints.client_details}/${details.id}`,
        details,
        details.clientId
      ).then((res) => {
        let newlyUpdatedItem = res.config.data;
        //Update clients list
        this.updateClientList();
      });
    },
    updateClientList() {
      if (this.$parent.searchWord.length === 0) {
        this.fetchClients();
      }
    },
    handleSlideOut() {
      this.turnOnSlideOut();
    },
    returnToDetails(client) {
      this.returnDetails(this.clients, this.itemId);
    },
    displayClientDetails(event) {
      this.displayItemDetails(this.clients);
    },
    fetchClients() {
      this.fetchItems(endpoints.client_details).then((res) => {
        let items = res.data;
        this.clients = items;
        this.oldClients = this.clients;
      });
    },
    searchClient(keyword) {
      this.clients = this.searchInBothArrayAndEndpoint(
        keyword,
        this.oldClients,
        endpoints.client_details
      );
    },
    createClient() {
      this.createItem(
        endpoints.client_details,
        this.newClient,
        this.newClient.clientId
      ).then((res) => {
        let newlyAddedClient = res.data;
        //Clear selected client
        this.selectedItem = newlyAddedClient;
        this.updateClientList();
        this.newClient = api_client_model;
      });
    },
  },
};
</script>
