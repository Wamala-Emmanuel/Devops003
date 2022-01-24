<template>
  <div>
    <!-- SlideOut component -->
    <slideout ref="slideout" @closeSlideout="turnOffSlideOut">
      <template v-slot:form>
        <div class="new-form" :class="{ 'slide-out-content': isFormContent }">
          <h2 class="slide-out-title">Add {{ formTitle }} resource</h2>
          <!-- JSON Editor -->
          <v-jsoneditor v-model="newResource" :plus="false"></v-jsoneditor>
          <!-- Action buttons -->
          <div class="row">
            <div class="col-sm-6">
              <div class="form-group text-right">
                <button
                  class="btn btn-w-m btn-primary mt-5 cancel"
                  id="cancel-update"
                  type="Submit"
                  @click.prevent="turnOffSlideOut"
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
                  @click.prevent="createResource"
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
          <h2 class="slide-out-title">Resource Details</h2>
          <div class="resource-view-container">
            <!-- json pretty Viewer -->
            <div class="pretty-container">
              <vue-json-pretty
                :path="'res'"
                :data="selectedItem"
                :show-length="true"
                :highlightMouseoverNode="true"
              >
              </vue-json-pretty>
            </div>
          </div>
          <!-- Action buttons -->
          <div class="row">
            <div class="col-sm-6">
              <div class="form-group text-right">
                <button
                  class="btn btn-w-m btn-primary mt-5 cancel"
                  id="cancel"
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
                  id="edit-resource"
                  type="edit-resource"
                  @click="editResource(selectedItem)"
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
          <form role="form" id="edit-user">
            <div class="row">
              <h2 class="slide-out-title">Edit resource</h2>
            </div>
            <!-- JSON Editor -->
            <v-jsoneditor v-model="detailsToEdit" :plus="false"></v-jsoneditor>
            <!-- Action buttons -->
            <div class="row">
              <div class="col-sm-6">
                <div class="form-group text-right">
                  <button
                    class="btn btn-w-m btn-primary mt-5 cancel"
                    id="return-to-details"
                    @click.prevent="returnToDetails"
                  >
                    Cancel
                  </button>
                </div>
              </div>
              <div class="col-sm-6">
                <div class="text-right">
                  <button
                    class="btn btn-w-m btn-primary mt-5 submit"
                    id="submit-user-update"
                    type="Submit"
                    @click.prevent="updateResource"
                  >
                    <i v-if="loadSpinner" class="fa fa-spinner fa-spin"></i>
                    Save
                  </button>
                </div>
              </div>
            </div>
          </form>
        </div>
      </template>
    </slideout>
    <!-- Breadcrumb -->
    <div class="row wrapper border-bottom white-bg page-heading">
      <breadcrumb :title="breadcrumb.title" :items="breadcrumb.items" />
      <EnvBanner />
      <div class="col-lg-5 text-right">
        <!-- Resources toggle button -->
        <div
          class="btn-group btn-group-sm"
          role="group"
          aria-label="Button group for resources"
        >
          <button
            type="button"
            class="btn mt-4"
            data-toggle="button"
            :class="{ 'btn-info': isApiResource }"
            @click="showApiResource"
          >
            API Resources
          </button>
          <button
            type="button"
            class="btn btn-light mt-4"
            data-toggle="button"
            :class="{ 'btn-info': isIdentityResource }"
            @click="showIdentityResource"
          >
            Identity Resources
          </button>
        </div>
        <!-- Add Resources dropdown button -->
        <button
          class="btn btn-info btn-sm dropdown-toggle mt-4"
          id="add-resource"
          type="button"
          data-toggle="dropdown"
          aria-haspopup="true"
          aria-expanded="false"
          aria-label="Add resource"
        >
          Add Resource
        </button>
        <div class="dropdown-menu dropdown-menu-right">
          <a
            class="dropdown-item"
            ref="new-user"
            @click="handleSlideOut('API', newAPIResource)"
          >
            API Resource
          </a>
          <div class="dropdown-divider"></div>
          <a
            class="dropdown-item"
            ref="new-user"
            @click="handleSlideOut('Identity', newIdentityResource)"
          >
            Identity Resource
          </a>
        </div>
      </div>
    </div>
    <!-- Resource table -->
    <div class="wrapper wrapper-content">
      <Table
        class="table-sm"
        ref="table"
        :theadings="resources_table.headings"
        :displayDetails="displayResourceDetails"
        :rows="tableResource"
        :allRows="tableResource"
        :fields="resources_table.fields"
        @edit="setResourceToEdit"
      >
      </Table>
    </div>
    <router-view></router-view>
  </div>
</template>

<script>
import Breadcrumb from "../components/shared/Breadcrumb.vue";
import Table from "../components/shared/Table";
import Slideout from "../components/shared/Slideout";
import sharedMethodsMixin from "@/mixins/sharedMethodsMixin";
import {
  endpoints,
  request,
  resource_labels,
  api_resource_model,
  identity_resource_model,
} from "../utils/constants";
import axios from "axios";
import VueJsonPretty from "vue-json-pretty";
import EnvBanner from "@/components/EnvBanner.vue";

export default {
  name: "Resources",
  mixins: [sharedMethodsMixin],
  components: {
    Slideout,
    Breadcrumb,
    Table,
    VueJsonPretty,
    EnvBanner,
  },
  data() {
    return {
      showLength: true,
      isApiResource: true,
      isIdentityResource: false,
      isValid: true,
      jsontext: "",
      formTitle: "",
      editType: "",
      detailsToEdit: {},
      isResourceActive: true,
      newResource: {},
      editedResource: {},
      newAPIResource: api_resource_model,
      newIdentityResource: identity_resource_model,
      apiResources: [],
      identityResources: [],
      tableResource: null,
      allResources: [],
      breadcrumb: {
        title: "Resources",
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
            label: "Resources",
            isActive: true,
          },
        ],
      },
      resources_table: {
        headings: [
          resource_labels.name,
          resource_labels.displayName,
          resource_labels.description,
          resource_labels.enabled,
        ],
        fields: ["$.name", "$.displayName", "$.description", "$.enabled"],
      },
      displayUrl: "",
      searchUrl: "",
      postUrl: "",
      resourceArray: [],
    };
  },
  created() {
    // call the fetch resources method
    this.fetchResources();
  },
  methods: {
    // toggle function for showing API resources
    showApiResource() {
      this.tableResource = this.apiResources;
      this.isApiResource = true;
      this.isIdentityResource = false;
    },
    // toggle function for showing Identity resources
    showIdentityResource() {
      this.tableResource = this.identityResources;
      this.isIdentityResource = true;
      this.isApiResource = false;
    },
    // set resource to edit
    setResourceToEdit() {
      // Check for which resource is being displayed in the table and set the URL
      if (this.tableResource == this.apiResources) {
        // set resource array to be of api resources
        this.resourceArray = this.apiResources;
      } else {
        // set resource array to be of identity resources
        this.resourceArray = this.identityResources;
      }
      this.setItemToEdit(this.resourceArray);
      // populating edit resource with details
      this.editResource(this.selectedItem);
      // activating the slideOut
      this.$refs.slideout.toggleSlideout();
    },
    // handle Edit resource slot template
    editResource(resource) {
      this.editItem(resource);
    },
    // Add resource function
    handleSlideOut(resourceType, resourceModel) {
      this.formTitle = resourceType;
      this.newResource = resourceModel;
      this.turnOnSlideOut();
    },
    // display resource details
    displayResourceDetails(event) {
      // Check for which resource is being displayed in the table and set the URL
      if (this.tableResource == this.apiResources) {
        // set resource array to be of api resources
        this.resourceArray = this.apiResources;
      } else {
        // set resource array to be of identity resources
        this.resourceArray = this.identityResources;
      }
      this.displayItemDetails(this.resourceArray);
    },
    // go back to where the edit function was called from
    returnToDetails() {
      this.returnDetails(this.resourceArray, this.itemId);
    },
    // fetch all resources from the given endpoints
    fetchResources() {
      // getting ApiResources
      this.fetchItems(endpoints.apiresource_details).then((res) => {
        let items = res.data;
        this.apiResources = items;
        this.allResources[0] = this.apiResources;
        this.tableResource = this.apiResources;
      });
      // getting identityResources
      this.fetchItems(endpoints.identityresource_details).then((res) => {
        let items = res.data;
        this.identityResources = items;
        this.allResources[1] = this.identityResources;
      });
    },
    // Search for an API resource
    searchApiResources(keyword) {
      this.tableResource = this.searchInBothArrayAndEndpoint(
        keyword,
        this.allResources[0],
        endpoints.apiresource_details
      );
    },
    // search in Identity resources array
    searchIdentityResources(keyword) {
      this.tableResource = this.searchInBothArrayAndEndpoint(
        keyword,
        this.allResources[1],
        endpoints.identityresource_details
      );
    },
    // create a resource
    createResource() {
      //assign Post URL
      if (this.formTitle == "API") {
        this.postUrl = endpoints.apiresource_details;
      } else this.postUrl = endpoints.identityresource_details;
      //Make post request
      this.createItem(
        this.postUrl,
        this.newResource,
        this.newResource.Name
      ).then((res) => {
        let newlyAddedResource = this.newResource;
        //Clear selected Resource
        let randomId = Math.floor(Math.random() * (10 - 0)) + 0;
        this.selectedItem = newlyAddedResource;
        this.selectedItem.id = randomId;

        this.editSelectedItem = false;
        this.fetchResources();
        // set which resources to display after creating resource
        if (this.postUrl === endpoints.apiresource_details) {
          this.showApiResource();
        } else this.showIdentityResource();
        //Reset form values
        this.newResource = {};
      });
    },
    // Edit a resource
    updateResource() {
      if (this.tableResource == this.apiResources) {
        this.postUrl = endpoints.apiresource_details;
      } else this.postUrl = endpoints.identityresource_details;
      this.editedResource = this.detailsToEdit;
      this.updateItemDetails(
        `${this.postUrl}/${this.editedResource.id}`,
        this.editedResource,
        "resource"
      ).then((res) => {
        let newlyUpdatedItem = res;
        //Update clients list
        this.fetchResources();
        //Reset form values
        this.editedResource = {};
      });
    },
  },
};
</script>
<style scoped>
.dropdown-menu {
  padding: 0.2rem 0;
}

.dropdown-divider {
  margin: 0.2rem 0;
}

.required:after {
  position: relative;
}

#add-resource {
  margin-left: 30px;
}
</style>
