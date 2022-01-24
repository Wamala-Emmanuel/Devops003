<template>
    <dl class="roles-description">
      <dt>System Administrator</dt>
      <dd>
        This is the ONLY role that creates and manages users and their access rights across the entire ID verification system. 
        This user does not access the ID verification portal.
      </dd>

      <dt>Portal Administrator</dt>
      <dd>
        This role can access all the modules and functionality in the ID Verification portal. 
        This role can make verification requests, export previous verification requests, and managing the settings within the portal.
      </dd>
      <dt>NIN Verifier</dt>
      <dd>
        This role can access the ID verification portal and ONLY has access rights to make and view ID verification requests.
      </dd>
    </dl>
</template>

<script>
    export default {
        name: "RoleDescription",
        
    }
</script>
