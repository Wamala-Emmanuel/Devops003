<template>
  <div>
    <slideout
      ref="slideout"
      @closeSlideout="handleCloseSlideout"
      :loadSlideOutSpinner="loadSlideOutSpinner"
      :isFormOpen="isFormOpen"
    >
      <template v-slot:form>
        <div class="new-form" :class="{ 'slide-out-content': isFormContent }">
          <h2 class="slide-out-title">Add User</h2>
          <form role="form" id="create-user" class="new-participant">
            <div class="row">
              <div class="col-sm-12">
                <div
                  class="form-group"
                  :class="{ hasError: $v.fullName.$error }"
                >
                  <label for="fullName">Full Name</label>
                  <input
                    type="text"
                    v-model.trim="fullName"
                    id="fullname"
                    @input="$v.fullName.$touch()"
                    :class="[
                      'form-control',
                      { 'has-error': $v.fullName.$error },
                    ]"
                  />
                  <div v-if="$v.fullName.$error">
                    <p class="error" v-if="!$v.fullName.required">
                      Full name is required
                    </p>
                  </div>
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-sm-12">
                <div class="form-group" :class="{ hasError: $v.email.$error }">
                  <label for="email">Email</label>
                  <input
                    type="email"
                    id="email"
                    v-model.trim="email"
                    @keyup="validateEmail(email)"
                    :class="[
                      'form-control',
                      { 'is-invalid': validateEmail(email) },
                    ]"
                    @input="$v.email.$touch()"
                  />
                  <div v-if="$v.email.$error">
                    <p class="error" v-if="!$v.email.required">
                      Email is required
                    </p>
                  </div>
                  <span :class="['warn', { display: validateEmail(email) }]"
                    >Invalid email</span
                  >
                </div>
              </div>
            </div>
            <!-- <div :class="['row']">
              <div class="col-sm-6">
                <div
                  class="form-group"
                  :class="{ hasError: $v.password.$error }"
                >
                  <label for="password">Password</label>
                  <input
                    type="password"
                    id="password"
                    v-model.trim="password"
                    required
                    minlength="8"
                    :class="['form-control']"
                    @input="$v.password.$touch()"
                  />
                  <div v-if="$v.password.$error">
                    <p class="error" v-if="!$v.password.required">
                      Password is required
                    </p>
                    <p
                      class="error password-error"
                      v-if="!$v.password.strongPassword"
                    >
                      Password must have a letter, number and a special
                      character (atleast 8)
                    </p>
                  </div>
                </div>
              </div>
              <div class="col-sm-6">
                <div
                  class="form-group"
                  :class="{ hasError: $v.confirmPassword.$error }"
                >
                  <label for="password2">Confirm Password</label>
                  <input
                    type="password"
                    id="password2"
                    required
                    minlength="5"
                    v-model.trim="confirmPassword"
                    :class="['form-control']"
                    @input="$v.confirmPassword.$touch()"
                  />
                  <div v-if="$v.confirmPassword.$error">
                    <p class="error" v-if="!$v.confirmPassword.required">
                      Password is required
                    </p>
                    <p class="error" v-if="!$v.confirmPassword.sameAsPassword">
                      The passwords do not match.
                    </p>
                  </div>
                </div>
              </div>
            </div> -->
            <div class="row">
              <div class="col-sm-12">
                <div class="form-group">
                  <label for="roles" :class="{ required: $v.role.$invalid }"
                    >User Role</label
                  >
                  <select
                    class="form-control"
                    v-model="role"
                    name="roles"
                    id="role"
                    required
                  >
                    <option value="sys_admin">System Administrator</option>
                    <option value="participant_super_user">
                      Portal Administrator
                    </option>
                    <option value="nin_verifier">NIN Verifier</option>
                  </select>
                  <p class="error" v-if="!$v.role.required">Role is required</p>
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-sm-6">
                <div class="form-group text-right">
                  <button
                    class="btn btn-w-m btn-primary mt-5 cancel"
                    id="cancel-update"
                    type="Submit"
                    @click.prevent="handleCloseSlideout"
                  >
                    Cancel
                  </button>
                </div>
              </div>
              <div class="col-sm-6">
                <div class="form-group text-right">
                  <button
                    class="btn btn-w-m btn-primary mt-5 submit"
                    id="submit-user"
                    type="submit"
                    @click.prevent="createUser"
                  >
                    <i v-if="loadSpinner" class="fa fa-spinner fa-spin"></i>
                    Save
                  </button>
                </div>
              </div>
            </div>
          </form>
          <role-description />
        </div>
      </template>
      <template v-slot:details v-if="selectedItem.id">
        <div
          class="user-details"
          :class="{ 'slide-out-content': isDetailsContent }"
        >
          <div class="row">
            <div class="col-sm-7">
              <h2 class="slide-out-title">User Details</h2>
            </div>
            <div class="col-sm-5 text-right status-switch">
              <ToggleButton
                :userStatus="!selectedItem.lockedOut"
                @activate-deactivate="changeUserStatus"
              />
            </div>
          </div>
          <div>
            <div class="row pt-2 pb-2">
              <div class="col-sm-4 details-label">ID</div>
              <div class="col-sm-8">{{ selectedItem.id }}</div>
            </div>
            <div class="row pt-2 pb-2">
              <div class="col-sm-4 details-label">Full Name</div>
              <div class="col-sm-8">{{ selectedItem.claims.name }}</div>
            </div>
            <div class="row pt-2 pb-2">
              <div class="col-sm-4 details-label">Email</div>
              <div class="col-sm-8">
                {{ selectedItem.email
                }}<span v-if="selectedItem.email == null">{{
                  selectedItem.claims.email
                }}</span>
              </div>
            </div>
            <div class="row pt-2 pb-2">
              <div class="col-sm-4 details-label">User Role</div>
              <div class="col-sm-8">
                {{
                  userRoles[
                    selectedItem.roles[0] && selectedItem.roles[0].toLowerCase()
                  ]
                }}
              </div>
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
                    class="btn btn-w-m btn-primary mt-4 edit"
                    type="button"
                    @click="editUser(selectedItem)"
                  >
                    Edit
                  </button>
                </div>
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
          <h2 class="slide-out-title">Edit user</h2>
          <form role="form" id="edit-user" class="new-participant">
            <div class="row">
              <div class="col-sm-12">
                <div
                  class="form-group"
                  :class="{ hasError: $v.fullName.$error }"
                >
                  <label for="fullNameForEdit">Full Name</label>
                  <input
                    type="text"
                    v-model.trim="fullName"
                    id="fullNameForEdit"
                    @input="$v.fullName.$touch()"
                    :class="[
                      'form-control',
                      { 'has-error': $v.fullName.$error },
                    ]"
                  />
                  <div v-if="$v.fullName.$error">
                    <p class="error" v-if="!$v.fullName.required">
                      Full name is required
                    </p>
                  </div>
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-lg-12">
                <div
                  class="form-group"
                  :class="{ hasError: $v.detailsToEdit.email.$error }"
                >
                  <label for="edit-email">Email</label>
                  <input
                    type="email"
                    id="edit-email"
                    disabled
                    v-model="detailsToEdit.email"
                    @keyup="validateEmail(detailsToEdit.email)"
                    :class="[
                      'form-control',
                      { 'is-invalid': validateEmail(detailsToEdit.email) },
                    ]"
                    @input="$v.detailsToEdit.email.$touch()"
                  />
                  <div class="small">User account email can not be changed. Consider creating a new account</div>
                  <div v-if="$v.detailsToEdit.email.$error">
                    <p class="error" v-if="!$v.detailsToEdit.email.required">
                      Email is required
                    </p>
                  </div>
                  <span
                    :class="[
                      'warn',
                      { display: validateEmail(detailsToEdit.email) },
                    ]"
                    >Invalid email</span
                  >
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-sm-12">
                <div class="form-group">
                  <label
                    for="roleForEdit"
                    :class="{ required: !$v.role.required }"
                    >User Role</label
                  >
                  <select
                    class="form-control"
                    v-model="role"
                    name="roles"
                    id="roleForEdit"
                    required
                  >
                    <option value="sys_admin">System Administrator</option>
                    <option value="participant_super_user">
                      Portal Administrator
                    </option>
                    <option value="nin_verifier">NIN Verifier</option>
                  </select>
                  <p class="error" v-if="!$v.role.required">Role is required</p>
                </div>
              </div>
            </div>

            <div class="row">
              <div class="col-sm-6">
                <div class="form-group text-right">
                  <button
                    class="btn btn-w-m btn-primary mt-5 cancel"
                    id="cancel-update"
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
                    @click.prevent="updateUserDetails(detailsToEdit)"
                  >
                    <i v-if="loadSpinner" class="fa fa-spinner fa-spin"></i>
                    Save
                  </button>
                </div>
              </div>
            </div>
          </form>
          <role-description />
        </div>
      </template>
    </slideout>

    <div class="row wrapper border-bottom white-bg page-heading">
      <breadcrumb :title="breadcrumb.title" :items="breadcrumb.items" />
      <EnvBanner />
      <add-button :button_label="button_label" @slideOut="turnOnSlideOut" />
    </div>
    <div class="wrapper wrapper-content">
      <Table
        ref="table"
        :theadings="users_table.headings"
        :displayDetails="displayUserDetails"
        :rows="users"
        :allRows="allUsers"
        :fields="users_table.fields"
        @edit="displayUserDetails"
        @filterData="displayRows($event)"
        @setPageNumber="goToPage($event)"
      >
      </Table>
    </div>
  </div>
</template>

<script>
import swal from "sweetalert";
import Breadcrumb from "../components/shared/Breadcrumb.vue";
import AddButton from "../components/shared/AddButton";
import Table from "../components/shared/Table";
import Slideout from "../components/shared/Slideout";
import ToggleButton from "../components/shared/ToggleButton";
import sharedMethodsMixin from "@/mixins/sharedMethodsMixin";
import RoleDescription from "../components/RoleDescription";
// import Multiselect from 'vue-multiselect'
import { endpoints, user_labels, request } from "../utils/constants";
import { validationMixin } from "vuelidate";
import {
  required,
  minLength,
  sameAs,
  requiredIf,
} from "vuelidate/lib/validators";
import { mask } from "vue-the-mask";
import EnvBanner from "@/components/EnvBanner.vue";

export default {
  name: "Users",
  mixins: [sharedMethodsMixin, validationMixin],
  components: {
    Slideout,
    Breadcrumb,
    AddButton,
    Table,
    // Multiselect,
    EnvBanner,
    ToggleButton,
    RoleDescription,
  },
  data() {
    return {
      itemsPerPage: 10,
      pageNumber: 1,
      detailsToEdit: {
        preferredUsername: "",
        email: "",
        fullName: "",
        claims: [],
      },
      userRoles: {
        participant_super_user: "Portal Administrator",
        sys_admin: "System Administrator",
        nin_verifier: "NIN Verifier",
      },
      deactivateParticipant: {
        title: "Are you sure?",
        text: "This user will not be able to log into the ID Verification application until the account is activated again.",
        icon: require("../assets/img/auth_images/disabled.svg"),
        buttons: {
          cancel: {
            text: "CANCEL",
            visible: true,
            className: "deactivate-cancel",
            closeModal: true,
          },
          confirm: {
            text: "DEACTIVATE",
            value: "confirm",
            visible: true,
            className: "deactivate-confirm",
            closeModal: true,
          },
        },
      },
      activateParticipant: {
        title: "Are you sure?",
        text: "This user account will be activated again.",
        icon: require("../assets/img/auth_images/check.png"),
        buttons: {
          cancel: {
            text: "CANCEL",
            visible: true,
            className: "deactivate-cancel",
            closeModal: true,
          },
          confirm: {
            text: "ACTIVATE",
            value: "confirm",
            visible: true,
            className: "activate-confirm",
            closeModal: true,
          },
        },
      },
      toastMessages: {
        activateParticipant: "Participant user has been successfully activated",
        deactivateParticipant: "Participant user has been successfully deactivated",
      },
      trashImage: { imgSource: "../assets/img/auth_images/trash.svg" },
      isUserActive: true,
      userStatus: "Active",
      fullName: "",
      userName: "",
      phoneNumber: "",
      organisationId: "",
      organisationName: "",
      participantId: "",
      participantName: "",
      branchId: "",
      branchName: "",
      password: "TEMPpwd1234,.",
      confirmPassword: "TEMPpwd1234,.",
      email: "",
      emailInvalid: null,
      phoneNumberInvalid: null,
      passwordInvalid: null,
      myRoles: [],
      role: "sys_admin",
      roles: "sys_admin",
      submitStatus: null,
      allRoles: [
        { name: "Admin", code: "admin" },
        { name: "Super admin", code: "superAdmin" },
      ],
      customClaims: [
        {
          typeOne: "",
          valueOne: "",
        },
      ],
      oldClaimDetails: {},
      users: null,
      allUsers: null,
      breadcrumb: {
        title: "Users",
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
            label: "Users",
            isActive: true,
          },
        ],
      },
      // button_label: user_labels.user,
      button_label: "User",
      users_table: {
        headings: [
          // user_labels.id,
          user_labels.name,
          user_labels.email,
          user_labels.role,
          user_labels.lockedOut,
        ],
        fields: [
          // "$.id",
          "$.claims.name",
          "$.claims.email",
          "$.roles[0]",
          "$.lockedOut",
        ],
      },
      phoneMask: "##########",
      formErrors: false,
      loading: false,
      loadSlideOutSpinner: true,
      isFormOpen: true,
    };
  },

  created() {
    this.fetchUsers(this.pageNumber, this.itemsPerPage);
    this.fetchAllUsers();
  },
  methods: {
    displayRows(e) {
      // Reset page number
      this.pageNumber = 1;
      this.itemsPerPage = e;
      this.fetchUsers(this.pageNumber, this.itemsPerPage);
    },
    goToPage(e) {
      this.pageNumber += e;
      this.fetchUsers(this.pageNumber, this.itemsPerPage);
    },
    setUserToEdit() {
      this.itemId = event.target.dataset.id;
      //Find matching user in users
      for (let i = 0; i < this.users.length; i++) {
        Object.keys(this.users[i]).forEach((key) => {
          if (this.users[i][key] == this.itemId) {
            //Set user as selected user
            this.selectedItem = this.users[i];
          }
        });
      }
      // search using id
      request
        .get(`${endpoints.user_details}/${this.itemId}`, {
          // Overrides the the default bearer. Doesnt reload upon login
          headers: this.headersOveride,
        })
        .then((response) => {
          this.selectedItem = response.data;
          this.editUser(this.selectedItem);
        })
        .catch(() => {
          console.log(`We're having trouble`);
        });
      //Turn on slideout
      this.$refs.slideout.toggleSlideout();
    },
    editUser(user) {
      //Get old values
      let oldDetails = user;
      //Convert custom claims to the form format
      let oldClaims = [];
      if (oldDetails.claims) {
        Object.keys(oldDetails.claims).forEach((key) => {
          oldClaims.push({
            typeOne: key,
            valueOne: oldDetails.claims[key],
          });
        });
        if (oldClaims.length == 0) {
          oldClaims.push({
            typeOne: "",
            valueOne: "",
          });
        }
      }
      this.detailsToEdit = user;
      this.detailsToEdit.claims = oldClaims;
      // transform claims to be read as variables on their own
      this.detailsToEdit.claims.forEach((claim) => {
        if (claim.typeOne === "name") {
          this.fullName = claim.valueOne || "";
        }
        this.detailsToEdit[`${claim.typeOne}`] = claim.valueOne;
      });

      this.role =
        this.detailsToEdit.roles[0] &&
        this.detailsToEdit.roles[0].toLowerCase();

      // set values for validation
      this.editItem(this.detailsToEdit);
    },
    updateUserDetails(details) {
      this.$v.$touch();
      this.loadSpinner = true;
      // transform userclaims form data into a userclaims object
      let userClaims = {
        email: details.email,
        name: this.fullName,
      };
      // validate other field values
      this.formErrors =
        this.$v.fullName.$invalid ||
        this.$v.role.$invalid ||
        this.$v.detailsToEdit.preferredUsername.$invalid ;

      // validate email if it exists
      if (this.detailsToEdit.email) {
        // if its invalid return user to addUser slide-out
        if (this.validateEmail(this.detailsToEdit.email)) {
          // hide load spinner and return to add new user slide-out
          this.loadSpinner = false;
          return;
        }
      }

      // update user details if other edit form values are valid
      if (this.formErrors === false) {
        let newUserDetails = {
          id: details.id,
          fullname: this.fullName,
          roles: [this.role],
          password: details.password,
          preferredUsername: details.preferredUsername,
          claims: JSON.parse(JSON.stringify(userClaims)),
        };
        this.updateItemDetails(
          endpoints.user_details,
          newUserDetails,
          newUserDetails.preferredUsername
        )
          .then(() => {
            this.loadSpinner = false;
            // Hide edit template
            (this.editSelectedItem = false),
              //Hide form
              (this.isFormContent = false);
            // Show details
            this.isDetailsContent = true;
            // reset form errors
            this.formErrors = false;
            //Show searched users or all users
            this.updateUserList();
          })
          .catch((error) => {
            this.loadSpinner = false;
            console.log("-----error------- \n" + error);
          });
      } else {
        // hide load spinner and return to add new user slide-out
        this.loadSpinner = false;
      }
    },
    updateUserList() {
      if (this.$parent.searchWord.length === 0) {
        this.fetchUsers(this.pageNumber, this.itemsPerPage);
      }
    },
    validateEmail(input) {
      let characters = /@\S./;
      if (input !== null && input !== undefined) {
        if (input.length > 0) {
          if (!characters.test(input)) {
            this.emailInvalid = true;
          } else {
            this.emailInvalid = false;
          }
        }
      }
      return this.emailInvalid;
    },
    validatePhoneNumber(input) {
      if (input) {
        this.phoneNumberInvalid = true;
      }
      return this.phoneNumberInvalid;
    },
    validatePassword(input) {
      if (input.length > 0 && input.length < 5) {
        this.passwordInvalid = true;
      } else {
        this.passwordInvalid = null;
      }
      return this.passwordInvalid;
    },
    changeUserStatus() {
      this.confirmationDialog(this.selectedItem.lockedOut)
    },
    activateDeactivateUser(userStatus) {
      request
        .put(
          `${endpoints.user_details}/${this.selectedItem.id}`,
          {
            lockedOut: userStatus,
          },
          {
            // Overrides the the default bearer. Doesnt reload upon login
            headers: this.headersOveride,
          }
        )
        .then(() => {
          if (this.selectedItem.lockedOut) {
            this.$toaster.success(this.toastMessages.activateParticipant, {
              timeout: 1000 * 5,
            });
          } else {
            this.$toaster.success(this.toastMessages.deactivateParticipant, {
              timeout: 1000 * 5,
            });
          }
          request
            .get(`${endpoints.user_details}/${this.selectedItem.id}`, {
              // Overrides the the default bearer. Doesnt reload upon login
              headers: this.headersOveride,
            })
            .then((response) => {
              this.selectedItem = response.data;
            })
            .catch((error) => {
              console.log(`${error}`);
            });
          this.fetchUsers();
        })
        .catch((error) => {
          console.log("-----error------- \n" + error);
        });
    },
    confirmationDialog(action) {
      let actionDetails = this.activateParticipant;
      if(!action) {
        actionDetails = this.deactivateParticipant
      }
      swal(actionDetails).then((continueTo) => {
        if (continueTo) {
          this.activateDeactivateUser(action = !action);
        }
      });
    },
    addRow(array) {
      array.push({
        typeOne: "",
        valueOne: "",
      });
    },
    deleteRow(array, index) {
      array.splice(index, 1);
    },
    handleCloseSlideout() {
      // clear form errors
      this.$v.$reset();
      this.turnOffSlideOut();
      //Reset form values
      this.userName = "";
      this.phoneNumber = "";
      this.password = "";
      this.confirmPassword = "";
      this.email = "";
      this.participantName = "";
      this.ParticipantId = "";
      this.customClaims = [
        {
          typeOne: "",
          valueOne: "",
        },
      ];
      this.emailInvalid = null;
      this.phoneNumberInvalid = null;
      this.passwordInvalid = null;
      this.passwordInvalid = null;
      this.usernameInvalid = null;
    },
    returnToDetails(user) {
      let userClaims = {};
      for (let i = 0; i < user.claims.length; i++) {
        Object.keys(user.claims[i]).forEach(() => {
          if (
            userClaims[user.claims[i].typeOne] !== "" &&
            user.claims[i].valueOne !== ""
          ) {
            userClaims[user.claims[i].typeOne] = user.claims[i].valueOne;
          }
        });
      }
      user.claims = userClaims;
      this.selectedItem = user;
      this.isFormContent = false;
      this.isDetailsContent = true;
      this.editSelectedItem = false;
    },
    displayUserDetails(event) {
      this.isFormOpen = false;
      this.loadSlideOutSpinner = true;
      this.itemId = event.target.dataset.id;
      this.selectedItem = {};
      for (let index = 0; index < this.users.length; index++) {
        if (this.users[index].id == this.itemId) {
          this.$refs.slideout.toggleSlideout();
          this.isFormContent = false;
          this.isDetailsContent = true;
          this.editSelectedItem = false;
          request
            .get(`${endpoints.user_details}/${this.users[index].id}`, {
              // Overrides the the default bearer. Doesnt reload upon login
              headers: this.headersOveride,
            })
            .then((response) => {
              this.selectedItem = response.data;
              if (!this.selectedItem.lockoutEnabled) {
                this.userStatus = "Deactivate";
              } else {
                this.userStatus = "Activate";
              }
              if (this.selectedItem.claims) {
                Object.keys(this.selectedItem.claims).forEach((key) => {
                  this.selectedItem[`${key}`] = this.selectedItem.claims[key];
                });
              }
              this.loadSlideOutSpinner = false;
              this.isFormOpen = true;
            })
            .catch((error) => {
              this.$toaster.success("Sorry " + error.response.data.Message);
              this.loadSlideOutSpinner = false;
              this.isFormOpen = true;
            });
        }
      }
    },
    fetchUsers(page, itemsPerPage) {
      page = this.pageNumber;
      itemsPerPage = this.itemsPerPage;
      let endpoint = `${endpoints.user_details}?Page=${page}`;
      if (itemsPerPage) endpoint += `&ItemsPerPage=${itemsPerPage}`;
      this.fetchItems(endpoint).then((res) => {
        let items = res.data;
        this.users = items;
        this.users.map((user) => {
          user.roles[0] = this.userRoles[
            user.roles[0] && user.roles[0].toLowerCase()
          ];
          user.lockedOut = user.lockedOut ? "Inactive" : "Active";
        });

        this.users.sort((rowOne, rowTwo) => {
          let firstItem = rowOne.claims.dateCreated
            ? new Date(rowOne.claims.dateCreated)
            : null;
          let secondItem = rowTwo.claims.dateCreated
            ? new Date(rowTwo.claims.dateCreated)
            : null;

          // if (firstItem === null) return -1;
          // if (secondItem === null) return 1;
          return secondItem - firstItem;

          // if (firstItem === undefined) return 1;
          // if (secondItem === undefined) return -1;

          // if (firstItem < secondItem) return -1;
          // if (firstItem > secondItem) return 1;
          // return 0;
        });
      });
    },
    fetchAllUsers(page = 1, itemsPerPage = undefined) {
      let endpoint = `${endpoints.user_details}?Page=${page}`;
      if (itemsPerPage) endpoint += `&ItemsPerPage=${itemsPerPage}`;
      this.fetchItems(endpoint).then((res) => {
        let items = res.data;
        this.allUsers = items;
      });
    },
    searchUser(keyword) {
      // search using id
      this.filterWithEndpoint(keyword, "Id", endpoints.user_details).then(
        (res) => {
          this.users = res.data;
          this.users.map((user) => {
            user.roles[0] = this.userRoles[
              user.roles[0] && user.roles[0].toLowerCase()
            ];
            user.lockedOut = user.lockedOut ? "Inactive" : "Active";
          });
        }
      );
      // search using username
      this.filterWithEndpoint(keyword, "userName", endpoints.user_details).then(
        (res) => {
          this.users = res.data;
          this.users.map((user) => {
            user.roles[0] = this.userRoles[
              user.roles[0] && user.roles[0].toLowerCase()
            ];
            user.lockedOut = user.lockedOut ? "Inactive" : "Active";
          });
        }
      );
       // search using name
      this.filterWithEndpoint(keyword, "Name", endpoints.user_details).then(
        (res) => {
          this.users = res.data;
          this.users.map((user) => {
            user.roles[0] = this.userRoles[
              user.roles[0] && user.roles[0].toLowerCase()
            ];
            user.lockedOut = user.lockedOut ? "Inactive" : "Active";
          });
        }
      );
      // search using username
      this.filterWithEndpoint(keyword, "Email", endpoints.user_details).then(
        (res) => {
          this.users = res.data;
          this.users.map((user) => {
            user.roles[0] = this.userRoles[
              user.roles[0] && user.roles[0].toLowerCase()
            ];
            user.lockedOut = user.lockedOut ? "Inactive" : "Active";
          });
        }
      );
      if (this.users.length === 50 && keyword.length > 2) {
        this.users = [];
      }
    },
    triggerResetPasswordLink(email) {
      if(email) {
        this.sendResetPasswordLink(
          endpoints.user_details + '/passwordreset',
          {"Email": email}
        )
      }
    },
    createUser() {
      this.$v.$touch();
      let newUser = {};
      let rawClaims = {
        email: this.email,
        dateCreated: new Date(),
      };
      let userClaims = rawClaims;
      this.formErrors =
        this.$v.fullName.$invalid ||
        this.$v.role.$invalid ||
        this.$v.email.$invalid;

      // validate email if it exists
      if (this.email) {
        // if its invalid return user to addUser slide-out
        if (this.validateEmail(this.email)) {
          this.loadSpinner = false;
          return;
        }
      }

      // Add a new user if the new user details have been validated
      if (this.formErrors === false) {
        this.loadSpinner = true;
        newUser = {
          fullname: this.fullName,
          roles: [this.role],
          preferredUsername: this.email,
          password: "TEMPpwd1234,.", // temporary
          claims: userClaims,
        };

        this.checkUserExists(
           endpoints.user_details + '/check',
           {"email": this.email}
        ).then((res) => {
          if(res.data){
            this.loadSpinner = false;
            this.$toaster.error("Sorry, this email is already in use. Try a different email", {
              timeout: 1000 * 5,
            });
          } else {

            this.createItem(
              endpoints.user_details,
              newUser,
              newUser.preferredUsername
              ).then((res) => {
                let newlyAddedUser = res.data;
                this.selectedItem = newlyAddedUser;
                this.selectedItem.name = newlyAddedUser.claims.name;
                this.selectedItem.email = newlyAddedUser.claims.email;
                this.triggerResetPasswordLink(newlyAddedUser.claims.email)
                this.updateUserList();
                //Reset form values
                this.fullName = "";
                this.userName = "";
                this.password = "";
                this.confirmPassword = "";
                this.email = "";
                this.customClaims = [
                  {
                    typeOne: "",
                    valueOne: "",
                  },
                ];
                // reset form errors
                this.formErrors = false;
              });
          }
        })

        
      } else {
        // hide load spinner and return to add new user slide-out
        this.loadSpinner = false;
      }
    },

    deleteUser(id) {
      this.deleteItem(`${endpoints.user_details}/${id}`).then(() => {
        this.fetchUsers(this.pageNumber, this.itemsPerPage);
      });
    },
  },
  validations: {
    fullName: {
      required,
    },
    userName: {
      required,
      minLength: minLength(5),
    },
    email: {
      required,
    },
    phoneNumber: {
      required,
      minLength: minLength(4),
    },
    password: {
      required,
      strongPassword(password) {
        return (
          /[a-zA-Z]/.test(password) && // checks for a-z
          /[0-9]/.test(password) && // checks for 0-9
          /\W|_/.test(password) && // checks for special character
          password.length >= 8 // should be at least 8 characters
        );
      },
    },
    confirmPassword: {
      required,
      sameAsPassword: sameAs("password"),
    },
    role: {
      required,
    },
    participantId: {
      required,
    },
    participantName: {
      required,
    },
    branchId: {
      required: requiredIf("branchName"),
    },
    branchName: {
      required: requiredIf("branchId"),
    },
    detailsToEdit: {
      fullName: {
        required,
      },
      email: {
        required,
      },
      preferredUsername: {
        required,
        minLength: minLength(5),
      },
      telephone: {
        required,
        minLength: minLength(4),
      },
      
    },
  },
  directives: {
    mask,
  },
};
</script>
<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>
<style scoped>
.warn,
.wrong-password,
.warn-update {
  display: none;
}

.warn.display,
.wrong-password.display,
.warn-update.display {
  display: block;
  color: red;
}
</style>
